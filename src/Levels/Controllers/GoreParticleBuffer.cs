using System.Collections.Generic;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class GoreParticleBuffer : Node2D
{
	[Export]
	private Texture2D _particleSprite = null!;

	[Export]
	private ShaderMaterial _goreShaderMaterial = null!;

	[ExportGroup("Internal")]
	// How much time to spend spawning particles
	[Export]
	public ulong RenderBudgetUs = 500;

	private static readonly StringName _gameTimeParam = "game_time";

	// Packed byte ranges, mirrored by unpack_w() in gore.gdshader.
	private const float PACKED_SCALE_MIN = 0.1f;
	private const float PACKED_SCALE_RANGE = 3.9f;
	private const float PACKED_SETTLE_MIN = 0.02f;
	private const float PACKED_SETTLE_RANGE = 1.73f;

	private MultiMeshInstance2D _mmiNode = null!;

	private GoreBurstParams _deathNormal = null!;
	private GoreBurstParams _deathExplosion = null!;
	private GoreBurstParams _spurt = null!;

	private readonly Queue<GoreParticle> _burstQueue = [];
	private MultiMesh _multiMesh = null!;

	private float _gameTime;

	// burst means a collection of particles. a particle is an individual sprite/droplet
	private int _maxBurstCount;
	private int _maxParticleCount;
	private int _activeParticles;

	private int _immediateParticlesSpawnedThisFrame;

	// the oldest particle gets reused when exceeding _maxParticleCount
	private int _nextParticleIdx;

	public void Initialize(
		int maxBurstCount,
		GoreBurstParams deathNormal,
		GoreBurstParams deathExplosion,
		GoreBurstParams spurt
	)
	{
		_deathNormal = deathNormal;
		_deathExplosion = deathExplosion;
		_spurt = spurt;

		_maxBurstCount = Math.Max(Math.Max(_deathNormal.Count, _deathExplosion.Count), _spurt.Count);
		_maxParticleCount = ToParticleCapacity(maxBurstCount);

		_multiMesh = CreateMultiMesh(Mathf.Max(1, _maxParticleCount));
		_mmiNode = new MultiMeshInstance2D
		{
			Name = "GoreParticleMultiMeshInstance2D",
			Multimesh = _multiMesh,
			Texture = _particleSprite,
			Material = _goreShaderMaterial,
		};
		AddChild(_mmiNode);
	}

	public override void _PhysicsProcess(double delta)
	{
		_immediateParticlesSpawnedThisFrame = 0;
		_gameTime += (float)delta;
		_goreShaderMaterial.SetShaderParameter(_gameTimeParam, _gameTime);

		SpawnQueuedParticles();
	}

	// spread out particle spawning across multiple frames to limit stuttering on large kills in a single frame
	private void SpawnQueuedParticles()
	{
		var start = Time.GetTicksUsec();
		while (_burstQueue.TryDequeue(out var particle))
		{
			if (Time.GetTicksUsec() - start > RenderBudgetUs)
				break;
			Write(in particle);
		}
	}

	public void SpawnDeathBurst(Vector2 position, DeathCauseEnum cause)
	{
		SpawnBurst(position, null, cause == DeathCauseEnum.Normal ? _deathNormal : _deathExplosion);
	}

	public void SpawnSpurtBurst(Vector2 position, float directionRadians)
	{
		SpawnBurst(position, directionRadians, _spurt);
	}

	private void SpawnBurst(Vector2 position, float? baseAngle, GoreBurstParams p)
	{
		if (_maxParticleCount <= 0)
			return;
		var spreadRadians = Mathf.DegToRad(p.SpreadDegrees);
		for (var i = 0; i < p.Count; i++)
		{
			var angle = baseAngle is { } direction
				? direction + (float)GD.RandRange(-spreadRadians, spreadRadians)
				: (float)GD.RandRange(0f, Mathf.Tau);
			var velocity = Vector2.FromAngle(angle) * (float)GD.RandRange(p.SpeedMin, p.SpeedMax);
			var origin = position + p.EmitOffset + RandomPointInCircle(p.EmitRadius);
			var particles = new GoreParticle(
				origin,
				velocity,
				_gameTime,
				(float)GD.RandRange(p.ScaleMin, p.ScaleMax),
				(float)GD.RandRange(p.SettleMin, p.SettleMax),
				RandomTint(p)
			);
			// Spawn some particles immediately for immediate visual feedback. Queue the rest
			if (_immediateParticlesSpawnedThisFrame < 1_000)
			{
				Write(in particles);
				_immediateParticlesSpawnedThisFrame++;
			}
			else
			{
				if (_burstQueue.Count > _maxParticleCount * 0.3f)
				{
					continue;
				}

				_burstQueue.Enqueue(particles);
			}
		}
	}

	public void SetCapacity(int targetCapacity)
	{
		_maxParticleCount = ToParticleCapacity(targetCapacity);

		_multiMesh.VisibleInstanceCount = 0;
		_multiMesh.InstanceCount = _maxParticleCount;

		_activeParticles = 0;
		_nextParticleIdx = 0;
		_burstQueue.Clear();
	}

	private void Write(in GoreParticle particle)
	{
		_activeParticles = Mathf.Clamp(_activeParticles + 1, 0, _maxParticleCount - 1);
		_nextParticleIdx = Mathf.Wrap(_nextParticleIdx + 1, 0, _maxParticleCount - 1);
		Upload(_nextParticleIdx, in particle);
		_multiMesh.VisibleInstanceCount = _activeParticles;
	}

	private void Upload(int idx, in GoreParticle particle)
	{
		_multiMesh.SetInstanceTransform2D(idx, new Transform2D(0f, particle.Origin));
		_multiMesh.SetInstanceCustomData(idx, PackCustomData(in particle));
		_multiMesh.SetInstanceColor(idx, particle.Tint);
	}

	// Packs scale/rotation/settle time as bytes in the w channel,
	// mirrored by unpack_w() in gore.gdshader.
	private static Color PackCustomData(in GoreParticle particle)
	{
		var scale = PackByte(particle.Scale, PACKED_SCALE_MIN, PACKED_SCALE_RANGE);
		var settle = PackByte(particle.SettleTime, PACKED_SETTLE_MIN, PACKED_SETTLE_RANGE);

		var bits = scale | (settle << 8);
		var packed = BitConverter.UInt32BitsToSingle(bits);
		return new Color(particle.Velocity.X, particle.Velocity.Y, particle.SpawnTime, packed);
	}

	private static uint PackByte(float value, float min, float range)
	{
		return (uint)Math.Clamp((value - min) / range * 255f, 0f, 255f);
	}

	private MultiMesh CreateMultiMesh(int capacity)
	{
		// Quad sized to the 3x2 small_chevron texture.
		return new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = true,
			UseCustomData = true,
			Mesh = new QuadMesh { Size = _particleSprite.GetSize() },
			InstanceCount = capacity,
			VisibleInstanceCount = 0,
		};
	}

	private int ToParticleCapacity(int maxBursts)
	{
		return Math.Max(0, maxBursts * Math.Max(1, _maxBurstCount));
	}

	private static Vector2 RandomPointInCircle(float radius)
	{
		var r = radius * Mathf.Sqrt((float)GD.RandRange(0f, 1f));
		return Vector2.FromAngle((float)GD.RandRange(0f, Mathf.Tau)) * r;
	}

	private static Color RandomTint(GoreBurstParams p)
	{
		var hue = Mathf.Wrap(p.BaseTint.H + (float)GD.RandRange(p.HueVariationMin, p.HueVariationMax), 0f, 1f);
		return Color.FromHsv(hue, p.BaseTint.S, p.BaseTint.V, p.BaseTint.A);
	}

	private readonly struct GoreParticle(
		Vector2 origin,
		Vector2 velocity,
		float spawnTime,
		float scale,
		float settleTime,
		Color tint
	)
	{
		public readonly Vector2 Origin = origin;
		public readonly Vector2 Velocity = velocity;
		public readonly float SpawnTime = spawnTime;
		public readonly float Scale = scale;
		public readonly float SettleTime = settleTime;
		public readonly Color Tint = tint;
	}
}
