// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Core.ECS;
using Game.Items.Offensive;
using Godot.Collections;

namespace Game.Levels.Controllers;

public partial class AirdropPlane : Node2D
{
	[Signal]
	public delegate void OnAirdropDroppedEventHandler(Vector2 dropPosition);

	public float Speed;

	private bool _isStarted;
	private float _timeToExpire;
	private float _t;
	private double _distanceToDrop;
	private double _displacement;
	private bool _hasDropped;
	private PackedScene? _dropScene;
	private Node? _dropParent;

	private const float DROP_INTERVAL_SEC = 0.2f;
	private float _timeUntilNextDrop = DROP_INTERVAL_SEC;
	private int _dropAmount = 10;

	public override void _Process(double delta)
	{
		if (!_isStarted)
			return;

		_t += (float)delta;
		Position += Vector2.Right.Rotated(Rotation) * Speed * (float)delta;
		_displacement += Speed * delta;

		if (_displacement > _distanceToDrop && !_hasDropped)
		{
			_timeUntilNextDrop -= (float)delta;
			if (_timeUntilNextDrop > 0 || _dropAmount-- <= 0)
				return;

			DropCarpetBomb();
			_timeUntilNextDrop = DROP_INTERVAL_SEC;
		}

		if (_t >= _timeToExpire)
			QueueFree();
	}

	public void Start(
		Vector2 startPosition,
		Vector2 dropPosition,
		float rotation,
		float speed,
		float timeToExpire,
		float dropPrecision = 2000,
		PackedScene? dropScene = null,
		Node? dropParent = null
	)
	{
		Rotation = rotation;
		GlobalPosition = startPosition;
		_distanceToDrop =
			startPosition.DistanceTo(dropPosition)
			+ GD.RandRange(dropPrecision / 2, dropPrecision) * GD.RandRange(-1, 1);
		Speed = speed;
		_isStarted = true;
		_timeToExpire = timeToExpire;
		_dropScene = dropScene;
		_dropParent = dropParent;
		var p = GD.Load<PackedScene>("uid://cq8d77rrxnkkv").Instantiate();
		AddChild(p);
	}

	private void Drop()
	{
		_hasDropped = true;
		if (_dropScene is not null)
		{
			var drop = _dropScene.InstantiateOrNull<Node2D>();
			if (drop is null)
			{
				CustomLogger.LogError("Failed to instantiate drop. Does it inherit from Node2D?");
				return;
			}

			drop.GlobalPosition = GlobalPosition;

			if (_dropParent is not null)
				_dropParent.CallDeferred(Node.MethodName.AddChild, drop);
			else
				GetTree().Root.CallDeferred(Node.MethodName.AddChild, drop);
		}

		CustomLogger.LogInfo($"Dropped at {GlobalPosition}, {_t}s");
		EmitSignalOnAirdropDropped(GlobalPosition);
	}

	private void DropCarpetBomb()
	{
		var rand = new Vector2(GD.RandRange(-1, 1), 0) * 200;
		var dropPos = GlobalPosition - rand.Rotated(Rotation);

		CustomLogger.LogInfo($"Dropped at {dropPos}, {_t}s");
		EnvironmentFxManager.PlayVfx(
			"explosion",
			new Dictionary<StringName, Variant> { { "position", dropPos }, { "scale", new Vector2(2, 2) } }
		);
		EnvironmentFxManager.PlaySfx("explosion_generic");

		if (!EnemyTargetQuery.Instance.TryGetTargetsInArea(dropPos, 400f, out var targets))
			return;

		foreach (var target in targets)
		{
			OffensiveEffects.ApplyDamage(target, 500, 0, 1, 1);
			OffensiveEffects.ApplyKnockback(target, dropPos, 100);
			GameWorld.World.Add(target, new DeathCauseComponent(DeathCauseEnum.Explosion));
		}
	}
}
