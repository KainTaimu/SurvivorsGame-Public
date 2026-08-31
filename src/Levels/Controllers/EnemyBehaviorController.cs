using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

[Flags]
public enum EnemyBehaviorTypes
{
	None,
	Lunger,
}

[GlobalClass]
public partial class EnemyBehaviorController : Node
{
	[Export]
	public EnemyBehaviorTypes SupportedTypes;

	[ExportCategory("Configuration")]
	[Export]
	public bool Enabled = true;

	private static Vector2 _cachedPlayerPosition;

	public override void _Process(double delta)
	{
		if (!Enabled)
			return;

		_cachedPlayerPosition = GameWorld.Instance.MainPlayer.GlobalPosition;
		if (SupportedTypes.HasFlag(EnemyBehaviorTypes.Lunger))
			ProcessLungersQuery(GameWorld.World, (float)delta);
	}

	[Query(Parallel = true)]
	[All<PositionComponent, VelocityComponent, LungerBehaviorDataComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ProcessLungers(
		[Data] in float delta,
		ref PositionComponent pos,
		ref VelocityComponent vel,
		ref LungerBehaviorDataComponent behavior
	)
	{
		behavior.TimeUntilNextLunge -= delta;
		if (behavior.TimeUntilNextLunge > 0)
			return;
		behavior.TimeUntilNextLunge = behavior.TimeBetweenLunges;

		if (
			_cachedPlayerPosition.DistanceSquaredTo(pos.Position)
			> behavior.MinDistanceToLunge * behavior.MinDistanceToLunge
		)
			return;
		vel.Velocity *= behavior.LungeVelocityMultiplier;
	}
}
