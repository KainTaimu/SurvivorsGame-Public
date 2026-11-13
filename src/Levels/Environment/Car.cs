// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Items.Offensive;
using Game.Levels.Controllers;
using Godot.Collections;

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
		if (targetQuery.TryGetTargetsInArea(GlobalPosition, _explosionRadius, out var targets))
		{
			foreach (var entity in targets)
			{
				if (!GameWorld.World.IsAlive(entity))
					continue;
				OffensiveEffects.ApplyDamage(entity, 100, 0, 0, 1);
				OffensiveEffects.ApplyKnockback(entity, GlobalPosition, 10);
			}
		}

		var data = new Dictionary<StringName, Variant> { { "position", Position } };
		EnvironmentFxManager.PlaySfx("car_explosion", data);
		EnvironmentFxManager.PlayVfx("car_explosion", data);
		OffensiveEffects.ApplyCameraShake(_cameraShakeScale, GetViewport, CreateTween);

		_debugPolygon.Color = Colors.DimGray;
		_body.ProcessMode = ProcessModeEnum.Disabled;
	}
}
