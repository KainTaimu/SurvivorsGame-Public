// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;
using CommunityToolkit.HighPerformance.Buffers;
using Game.Items.Offensive;
using Game.Levels.Controllers;

namespace Game.Levels.Environment;

public partial class Car : Node2D, IDestructable
{
	[Export]
	private int _health = 100;

	[Export]
	private int _explosionRadius = 512;

	[Export]
	private int _cameraShakeScale = 5;

	[ExportGroup("Internal")]
	[Export]
	private PackedScene _explosionScene = null!;

	[Export]
	private PhysicsBody2D _body = null!;

	[Export]
	private Polygon2D _debugPolygon = null!;

	private bool _dead;

	public void TakeHit(int damage)
	{
		if (_dead)
			return;
		_health -= damage;
		EnvironmentFxManager.PlaySfx("car_hit");
		if (_health <= 0)
			Explode();
	}

	private void Explode()
	{
		_dead = true;
		var targetQuery = EnemyTargetQuery.Instance;
		using var targets = SpanOwner<Entity>.Allocate(512);
		var hitCount = targetQuery.TryGetTargetsInArea(GlobalPosition, _explosionRadius, targets.Span, 512);
		if (hitCount <= 0)
			return;

		foreach (var entity in targets.Span)
		{
			if (!GameWorld.World.IsAlive(entity))
				continue;
			OffensiveEffects.ApplyDamage(entity, 100, 0, 0, 1);
			OffensiveEffects.ApplyKnockback(entity, GlobalPosition, 10);
		}

		var data = new Godot.Collections.Dictionary<StringName, Variant> { { "position", Position } };
		EnvironmentFxManager.PlaySfx("car_explosion", data);
		EnvironmentFxManager.PlayVfx("car_explosion", data);
		OffensiveEffects.ApplyCameraShake(_cameraShakeScale, GetViewport, CreateTween);

		_debugPolygon.Color = Colors.DimGray;
		_body.ProcessMode = ProcessModeEnum.Disabled;
	}
}
