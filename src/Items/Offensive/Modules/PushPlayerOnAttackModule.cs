// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Levels.Controllers;
using Game.Players;
using Game.UI;

namespace Game.Items.Offensive.Modules;

[GlobalClass]
public partial class PushPlayerOnAttackModule : Node
{
	[Export]
	private PushOnAttackStats PushOnAttackStats { get; set; } = null!;

	private static Player Player => GameWorld.Instance.MainPlayer;
	private static Crosshair? Crosshair => Crosshair.Instance;

	public void PushPlayer()
	{
		var playerPosition = Player.GetCanvasTransform() * Player.Position;
		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();
		var rotation = playerPosition.AngleToPoint(mouseVector);

		Player.GlobalPosition +=
			Vector2.Right.Rotated(rotation - Mathf.Pi) * PushOnAttackStats.PushPerAttack * (float)GetProcessDeltaTime();
	}

	public void PushPlayerWithRotation(float rotation, float bloom)
	{
		Player.GlobalPosition +=
			Vector2.Right.Rotated(rotation - Mathf.Pi + bloom)
			* PushOnAttackStats.PushPerAttack
			* (float)GetProcessDeltaTime();
	}
}
