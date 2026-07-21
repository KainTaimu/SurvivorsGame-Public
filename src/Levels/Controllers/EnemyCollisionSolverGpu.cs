using System.Buffers.Binary;
using System.Threading;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.UI;

namespace Game.Levels.Controllers;

/// <summary>
/// Compute-shader port of <see cref="EnemyCollisionSolverCpu"/>. Each
/// substep rebuilds the uniform grid on the GPU (count -> scan ->
/// scatter) and solves pushes gather-style. All substeps run
/// inside one compute list; the CPU syncs once per physics frame and
/// applies the results (including the NavMap clamp) as usual.
/// </summary>
public partial class EnemyCollisionSolverGpu : AbstractEnemyCollisionSolver
{
	private const int MAX_ENTITIES = 16384;
	private const int ENTITY_STRIDE = 16;
	private const int WORKGROUP_SIZE = 64;
	private const int PUSH_CONSTANT_SIZE = 48;

	[Export]
	private NavMap? _navMap;

	[ExportCategory("Configuration")]
	[Export]
	private byte _gridSize = 64;

	[Export]
	private float _solverRangeFactor = 3f;

	[Export]
	private int _distBeforeShove = 50;

	[Export(PropertyHint.Range, "0,5,0.1")]
	private float _pushAmount = 0.4f;

	[Export]
	private int _cramLimitBeforeExtraPush = 6;

	[Export]
	private float _cramExtraPushFactor = 6f;

	[ExportCategory("Toggles")]
	[Export]
	public bool Enabled = true;

	[Export]
	public byte SubSteps = 6;

	private RenderingDevice? _rd;

	private Rid _entitiesA;
	private Rid _entitiesB;
	private Rid _cellCount;
	private Rid _cellStart;
	private Rid _entityIndices;

	private Rid _countShader;
	private Rid _scanShader;
	private Rid _scatterShader;
	private Rid _solveShader;

	private Rid _countPipeline;
	private Rid _scanPipeline;
	private Rid _scatterPipeline;
	private Rid _solvePipeline;

	// Uniform sets per ping-pong parity: even substeps read A/write B,
	// odd substeps read B/write A. Scan never touches entity buffers.
	private readonly Rid[] _countSets = new Rid[2];
	private readonly Rid[] _scatterSets = new Rid[2];
	private readonly Rid[] _solveSets = new Rid[2];
	private Rid _scanSet;

	private Vector2 _windowSize;
	private Vector2 _gridTopLeft;
	private Vector2I _gridDims;
	private int _cellTotal;

	// Dense upload buffer indexed by Entity.Id. Layout matches the GLSL
	// GpuEntity struct: vec2 pos, float radius, uint frameStamp. Slots
	// are valid for the current frame when their stamp == _writeFrame.
	private readonly byte[] _entityBuffer = new byte[MAX_ENTITIES * ENTITY_STRIDE];

	// [0] = max written Entity.Id this frame, [1] = overflow flag.
	private readonly int[] _uploadState = new int[2];

	private readonly byte[] _pushConstants = new byte[PUSH_CONSTANT_SIZE];
	private int _writeFrame;
	private bool _overflowWarned;

	public override void _Ready()
	{
		var viewport = GetViewport();
		if (viewport is null)
		{
			Logger.LogError("EnemyCollisionSolverGpu: missing viewport.");
			return;
		}

		// Square window for the same "spill" reason as the CPU solver.
		var visible = viewport.GetVisibleRect().Size * _solverRangeFactor;
		_windowSize = new Vector2(visible.X, visible.X);
		_gridDims = new Vector2I((int)_windowSize.X / _gridSize, (int)_windowSize.Y / _gridSize);
		_cellTotal = _gridDims.X * _gridDims.Y;

		InitGpu();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled || _rd is null)
			return;

		var player = GameWorld.Instance.MainPlayer;
		_gridTopLeft = player.GlobalPosition - _windowSize * 0.5f;
		var bounds = new Rect2(_gridTopLeft, _windowSize);

		_writeFrame++;
		if (_writeFrame == int.MaxValue)
		{
			_writeFrame = 1;
			Array.Clear(_entityBuffer);
		}

		_uploadState[0] = -1;
		_uploadState[1] = 0;

		var start = Time.GetTicksUsec();
		using (FrameTime.Record())
		{
			AddObjectsToBufferQuery(GameWorld.World, _entityBuffer, _uploadState, _writeFrame, bounds);

			if (_uploadState[1] != 0 && !_overflowWarned)
			{
				_overflowWarned = true;
				Logger.LogError(
					"EnemyCollisionSolverGpu: Entity.Id exceeded " + MAX_ENTITIES,
					"; entities beyond the cap are not collision-solved."
				);
			}

			var maxId = _uploadState[0];
			if (maxId < 0)
			{
				FrameTime.ProcessTime = Time.GetTicksUsec() - start;
				return;
			}

			var entityCount = maxId + 1;
			var byteCount = (uint)(entityCount * ENTITY_STRIDE);
			_rd.BufferUpdate(_entitiesA, 0, byteCount, _entityBuffer);

			WritePushConstants(entityCount);

			var computeList = _rd.ComputeListBegin();
			for (var substep = 0; substep < SubSteps; substep++)
			{
				var parity = substep & 1;
				var groups = (uint)((entityCount + WORKGROUP_SIZE - 1) / WORKGROUP_SIZE);

				Dispatch(computeList, _countPipeline, _countSets[parity], groups);
				_rd.ComputeListAddBarrier(computeList);
				Dispatch(computeList, _scanPipeline, _scanSet, 1);
				_rd.ComputeListAddBarrier(computeList);
				Dispatch(computeList, _scatterPipeline, _scatterSets[parity], groups);
				_rd.ComputeListAddBarrier(computeList);
				Dispatch(computeList, _solvePipeline, _solveSets[parity], groups);
				_rd.ComputeListAddBarrier(computeList);
			}

			_rd.ComputeListEnd();

			_rd.Submit();
			// Results are needed by ApplyCollisions this same physics frame.
			_rd.Sync();

			// Substep 0 reads A and writes B, so after an even number of
			// substeps the final positions are back in A.
			var finalBuffer = SubSteps % 2 == 0 ? _entitiesA : _entitiesB;
			var results = _rd.BufferGetData(finalBuffer, 0, byteCount);

			ApplyCollisionsQuery(GameWorld.World, results, _writeFrame, _navMap!);
		}
	}

	public override void _ExitTree()
	{
		if (_rd is null)
			return;

		foreach (var set in _countSets)
			FreeRid(set);
		foreach (var set in _scatterSets)
			FreeRid(set);
		foreach (var set in _solveSets)
			FreeRid(set);
		FreeRid(_scanSet);

		FreeRid(_countPipeline);
		FreeRid(_scanPipeline);
		FreeRid(_scatterPipeline);
		FreeRid(_solvePipeline);

		FreeRid(_countShader);
		FreeRid(_scanShader);
		FreeRid(_scatterShader);
		FreeRid(_solveShader);

		FreeRid(_entitiesA);
		FreeRid(_entitiesB);
		FreeRid(_cellCount);
		FreeRid(_cellStart);
		FreeRid(_entityIndices);

		_rd.Free();
		_rd = null;
	}

	private void FreeRid(Rid rid)
	{
		if (rid.IsValid)
			_rd!.FreeRid(rid);
	}

	private void InitGpu()
	{
		_rd = RenderingServer.CreateLocalRenderingDevice();
		if (_rd is null)
		{
			Logger.LogError(
				"EnemyCollisionSolverGpu: no local RenderingDevice",
				"(RenderingDevice requires Forward+ or Mobile renderer).",
				"Solver disabled."
			);
			Enabled = false;
			return;
		}

		_countShader = LoadShader("res://src/Shaders/Compute/enemy_collision_count.glsl");
		_scanShader = LoadShader("res://src/Shaders/Compute/enemy_collision_scan.glsl");
		_scatterShader = LoadShader("res://src/Shaders/Compute/enemy_collision_scatter.glsl");
		_solveShader = LoadShader("res://src/Shaders/Compute/enemy_collision_solve.glsl");

		if (!_countShader.IsValid || !_scanShader.IsValid || !_scatterShader.IsValid || !_solveShader.IsValid)
		{
			Enabled = false;
			return;
		}

		// cellCount must start zeroed; the scatter pass drains it back
		// to zero every substep, upholding the invariant afterwards.
		_entitiesA = _rd.StorageBufferCreate(MAX_ENTITIES * ENTITY_STRIDE);
		_entitiesB = _rd.StorageBufferCreate(MAX_ENTITIES * ENTITY_STRIDE);
		_cellCount = _rd.StorageBufferCreate((uint)(_cellTotal * sizeof(int)), new byte[_cellTotal * sizeof(int)]);
		_cellStart = _rd.StorageBufferCreate((uint)((_cellTotal + 1) * sizeof(int)));
		_entityIndices = _rd.StorageBufferCreate(MAX_ENTITIES * sizeof(int));

		_countPipeline = _rd.ComputePipelineCreate(_countShader);
		_scanPipeline = _rd.ComputePipelineCreate(_scanShader);
		_scatterPipeline = _rd.ComputePipelineCreate(_scatterShader);
		_solvePipeline = _rd.ComputePipelineCreate(_solveShader);

		var entities = new[] { _entitiesA, _entitiesB };
		for (var parity = 0; parity < 2; parity++)
		{
			_countSets[parity] = CreateSet(_countShader, (0, entities[parity]), (2, _cellCount));
			_scatterSets[parity] = CreateSet(
				_scatterShader,
				(0, entities[parity]),
				(2, _cellCount),
				(3, _cellStart),
				(4, _entityIndices)
			);
			_solveSets[parity] = CreateSet(
				_solveShader,
				(0, entities[parity]),
				(1, entities[1 - parity]),
				(3, _cellStart),
				(4, _entityIndices)
			);
		}
		_scanSet = CreateSet(_scanShader, (2, _cellCount), (3, _cellStart));
	}

	private Rid LoadShader(string path)
	{
		var file = GD.Load<RDShaderFile>(path);
		if (file is null)
		{
			Logger.LogError("EnemyCollisionSolverGpu: missing shader ", path);
			Enabled = false;
			return default;
		}

		var spirv = file.GetSpirV();
		var compileError = spirv.GetStageCompileError(RenderingDevice.ShaderStage.Compute);
		if (!string.IsNullOrEmpty(compileError))
		{
			Logger.LogError("EnemyCollisionSolverGpu: shader error in ", path, compileError);
			Enabled = false;
			return default;
		}

		var shader = _rd!.ShaderCreateFromSpirV(spirv);
		if (!shader.IsValid)
		{
			Logger.LogError("EnemyCollisionSolverGpu: shader create failed ", path);
			Enabled = false;
		}
		return shader;
	}

	private Rid CreateSet(Rid shader, params (int Binding, Rid Buffer)[] entries)
	{
		var uniforms = new Godot.Collections.Array<RDUniform>();
		foreach (var (binding, buffer) in entries)
		{
			var uniform = new RDUniform { UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = binding };
			uniform.AddId(buffer);
			uniforms.Add(uniform);
		}
		return _rd!.UniformSetCreate(uniforms, shader, 0);
	}

	private void WritePushConstants(int entityCount)
	{
		var span = _pushConstants.AsSpan();
		BinaryPrimitives.WriteSingleLittleEndian(span, _gridTopLeft.X);
		BinaryPrimitives.WriteSingleLittleEndian(span[4..], _gridTopLeft.Y);
		BinaryPrimitives.WriteSingleLittleEndian(span[8..], _gridSize);
		BinaryPrimitives.WriteInt32LittleEndian(span[12..], _gridDims.X);
		BinaryPrimitives.WriteInt32LittleEndian(span[16..], _gridDims.Y);
		BinaryPrimitives.WriteInt32LittleEndian(span[20..], entityCount);
		BinaryPrimitives.WriteSingleLittleEndian(span[24..], _distBeforeShove);
		BinaryPrimitives.WriteSingleLittleEndian(span[28..], _pushAmount);
		BinaryPrimitives.WriteInt32LittleEndian(span[32..], _cramLimitBeforeExtraPush);
		BinaryPrimitives.WriteSingleLittleEndian(span[36..], _cramExtraPushFactor);
		BinaryPrimitives.WriteInt32LittleEndian(span[40..], _writeFrame);
		BinaryPrimitives.WriteInt32LittleEndian(span[44..], 0);
	}

	private void Dispatch(long computeList, Rid pipeline, Rid uniformSet, uint groups)
	{
		_rd!.ComputeListBindComputePipeline(computeList, pipeline);
		_rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
		_rd.ComputeListSetPushConstant(computeList, _pushConstants, PUSH_CONSTANT_SIZE);
		_rd.ComputeListDispatch(computeList, groups, 1, 1);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CollisionLodComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	private static void AddObjectsToBuffer(
		[Data] in byte[] buffer,
		[Data] in int[] uploadState,
		[Data] in int writeFrame,
		[Data] in Rect2 bounds,
		in Entity entity,
		ref PositionComponent pos,
		ref CircleHitboxComponent circle
	)
	{
		if (!bounds.HasPoint(pos.Position))
			return;

		var id = entity.Id;
		if ((uint)id >= MAX_ENTITIES)
		{
			Interlocked.Exchange(ref uploadState[1], 1);
			return;
		}

		var span = buffer.AsSpan(id * ENTITY_STRIDE, ENTITY_STRIDE);
		BinaryPrimitives.WriteSingleLittleEndian(span, pos.Position.X);
		BinaryPrimitives.WriteSingleLittleEndian(span[4..], pos.Position.Y);
		BinaryPrimitives.WriteSingleLittleEndian(span[8..], circle.Radius);
		BinaryPrimitives.WriteInt32LittleEndian(span[12..], writeFrame);

		int currentMax;
		while ((currentMax = Volatile.Read(ref uploadState[0])) < id)
			Interlocked.CompareExchange(ref uploadState[0], id, currentMax);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, CircleHitboxComponent>]
	[None<DyingMarkerComponent>]
	private static void ApplyCollisions(
		[Data] in byte[] results,
		[Data] in int writeFrame,
		[Data] in NavMap navMap,
		in Entity entity,
		ref PositionComponent pos
	)
	{
		var id = entity.Id;
		var offset = id * ENTITY_STRIDE;
		if ((uint)id >= MAX_ENTITIES || (uint)(offset + ENTITY_STRIDE) > (uint)results.Length)
			return;

		var span = results.AsSpan(offset, ENTITY_STRIDE);
		if (BinaryPrimitives.ReadInt32LittleEndian(span[12..]) != writeFrame)
			return;

		var newPos = new Vector2(
			BinaryPrimitives.ReadSingleLittleEndian(span),
			BinaryPrimitives.ReadSingleLittleEndian(span[4..])
		);

		// Arch does not support nullable operator in parameters
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (navMap is not null && navMap.GridVisibilityRect.HasPoint(pos.Position))
		{
			pos.Position = NavigationServer2D.MapGetClosestPoint(NavMap.Map, newPos);
		}
		else
			pos.Position = newPos;
	}
}
