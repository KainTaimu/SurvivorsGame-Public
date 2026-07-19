using System.Numerics;
using System.Runtime.InteropServices;
using Arch.Core;
using Game.Core.ECS;
using GodotVector2 = Godot.Vector2;

namespace Game.Levels.Controllers;

public partial class EnemyMover : Node
{
	private static double _processDelta;
	private static GodotVector2 _playerPosition;

	public override void _Process(double delta)
	{
		_processDelta = GetProcessDeltaTime();

		var player = GameWorld.Instance.MainPlayer;
		_playerPosition = player.GlobalPosition;

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

			var moveSpeeds = chunk.GetSpan<MoveSpeedComponent>();
			var velocities = chunk.GetSpan<VelocityComponent>();

			foreach (var entityIndex in chunk)
			{
				ref var pos = ref positions[entityIndex];
				ref var moveSpeed = ref moveSpeeds[entityIndex];
				ref var velocity = ref velocities[entityIndex];

				velocity.Velocity = velocity.Velocity.Lerp(
					pos.Position.DirectionTo(_playerPosition) * moveSpeed.MoveSpeed,
					(float)(moveSpeed.TurnSpeed * _processDelta)
				);
			}

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
