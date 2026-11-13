// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using Game.Core.ECS;
using Game.Items.Projectiles;

namespace Game.Levels.Controllers;

public partial class EnemyDeathManager : Node
{
	[Signal]
	public delegate void OnEnemyDeathEventHandler(EntityObject entity);

	private readonly ConcurrentQueue<Entity> _pendingDeaths = [];

	public override void _Process(double delta)
	{
		UpdateNewDeathsQuery(GameWorld.World);
		UpdateDyingQuery(GameWorld.World, (float)delta);

		while (_pendingDeaths.TryDequeue(out var entity))
		{
			if (!GameWorld.World.IsAlive(entity))
				continue;
			GameWorld.World.Destroy(entity);
		}
	}

	[Query]
	[All<HealthComponent, PositionComponent>]
	[None<DyingMarkerComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
	private void UpdateNewDeaths(Entity entity, ref HealthComponent health)
	{
		if (health.Health > 0)
			return;
		if (!GameWorld.World.IsAlive(entity))
			return;

		GameWorld.World.Add(entity, DyingMarkerComponent.Default);

		if (GameWorld.World.TryGet<DeathRewardComponent>(entity, out var reward))
			LevelData.Instance?.Money += reward.Money;

		EmitSignalOnEnemyDeath(new EntityObject(entity));
	}

	[Query]
	[All<DyingMarkerComponent, PositionComponent, VelocityComponent, AnimatedSpriteComponent>]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdateDying(
		[Data] in float delta,
		Entity entity,
		ref DyingMarkerComponent dying,
		ref PositionComponent pos,
		ref VelocityComponent vel,
		ref AnimatedSpriteComponent spr
	)
	{
		if (dying.TimeLeftUntilDestroy <= 0)
		{
			_pendingDeaths.Enqueue(entity);
			return;
		}

		dying.TimeLeftUntilDestroy -= delta;
		pos.Position += vel.Velocity * delta;
		spr.Flash = 255;
		spr.Opacity = (byte)(dying.TimeLeftUntilDestroy / DyingMarkerComponent.Default.TimeLeftUntilDestroy * 255);
	}
}
