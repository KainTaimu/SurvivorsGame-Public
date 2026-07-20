using System.Numerics;
using System.Runtime.InteropServices;
using Arch.Core;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	[Export]
	public bool Enabled = true;

	private static double _processDelta;

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("DISABLE_ENEMY_MOVEMENT"))
			Enabled = !Enabled;
	}

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

		_processDelta = GetProcessDeltaTime();

		MoveSimd();
	}

	private static readonly QueryDescription _targetQuery = new QueryDescription()
		.WithAll<PositionComponent, VelocityComponent, MoveSpeedComponent, CollisionLodComponent>()
		.WithNone<DyingMarkerComponent>();

	private static void MoveSimd()
	{
		var world = GameWorld.World;

		world.InlineParallelChunkQuery(in _targetQuery, new MoveUpdate());
	}

	public struct MoveUpdate : IChunkJob
	{
		public void Execute(ref Chunk chunk)
		{
			var positions = chunk.GetSpan<PositionComponent>();
			var velocities = chunk.GetSpan<VelocityComponent>();
			var posF = MemoryMarshal.Cast<PositionComponent, float>(positions);
			var velF = MemoryMarshal.Cast<VelocityComponent, float>(velocities);

			var dt = (float)_processDelta;
			var dtVec = new Vector<float>(dt);
			var count = Vector<float>.Count;
			var i = 0;
			for (; i <= posF.Length - count; i += count)
			{
				var v1 = new Vector<float>(posF.Slice(i, count));
				var v2 = new Vector<float>(velF.Slice(i, count));
				(v1 + v2 * dtVec).CopyTo(posF.Slice(i, count));
			}
			for (; i < posF.Length; i++)
			{
				posF[i] += velF[i] * dt;
			}
		}
	}
}
