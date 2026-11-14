// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Levels.Controllers;
using Game.Players.Controllers;
using Game.UI;

namespace Game.Items.Offensive.Modules;

// NOTE: This overwrites DamageStats and BloomStats directly
[GlobalClass]
public partial class MoveBloomModule : Node
{
	[Export]
	private Node Parent { get; set; } = null!;

	[Export]
	private DamageStats DamageStats { get; set; } = null!;

	[Export]
	private BloomStats BloomStats { get; set; } = null!;

	[Export]
	private MoveBloomStats MoveBloomStats { get; set; } = null!;

	[Export]
	private bool MovementCausesBloom { get; set; } = true;

	[Export]
	private bool CrosshairMovementCausesBloom { get; set; } = true;

	private static Crosshair? Crosshair => Crosshair.Instance;
	private PlayerMovementController MovementController => GameWorld.Instance.MainPlayer.MovementController;
	private AbstractPlayerWeaponController WeaponController => GameWorld.Instance.MainPlayer.WeaponController;

	public override void _Process(double delta)
	{
		if (WeaponController.PrimaryAttack == Parent)
			UpdateMoveTimeBloom((float)delta);
	}

	private void UpdateMoveTimeBloom(float delta)
	{
		if (
			MovementCausesBloom && MovementController.Velocity.LengthSquared() > 0
			|| CrosshairMovementCausesBloom && Crosshair?.CrosshairVelocity.LengthSquared() > 0
		)
			MoveBloomStats.MoveTime += delta * MoveBloomStats.MoveBloomGrowthRate;
		else
			MoveBloomStats.MoveTime -= delta * MoveBloomStats.MoveBloomShrinkRate;

		SpreadCrosshair((float)MoveBloomStats.MoveTimeFactor);

		UpdateDamageStats();
		UpdateBloom();
	}

	private void UpdateBloom()
	{
		var bloomDeg = MoveBloomStats.MoveBloomMaxDeg * MoveBloomStats.MoveTimeFactor;
		if (MoveBloomStats.IsPerfectBloom)
			bloomDeg = MoveBloomStats.MoveBloomPerfectBloomDeg;
		else
			bloomDeg = Math.Clamp(bloomDeg, MoveBloomStats.MoveBloomMinDeg, MoveBloomStats.MoveBloomMaxDeg);
		BloomStats.BloomCoefficientDeg = (float)bloomDeg;
	}

	private void UpdateDamageStats()
	{
		DamageStats.CritDamageChance = (float)(1 - MoveBloomStats.MoveTimeFactor);
		DamageStats.Damage = (int)
			Math.Clamp(
				MoveBloomStats.MoveDamageMax * (1 - MoveBloomStats.MoveTimeFactor),
				MoveBloomStats.MoveDamageMin,
				MoveBloomStats.MoveDamageMax
			);
	}

	private void SpreadCrosshair(float spreadRatio)
	{
		Crosshair?.ChangePrimaryCrosshairSpread(spreadRatio);
	}
}
