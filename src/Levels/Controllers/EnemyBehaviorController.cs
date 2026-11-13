// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.CompilerServices;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

[Flags]
public enum EnemyBehaviorTypes
{
	None = 0,
	Lunger = 1 << 1,
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
