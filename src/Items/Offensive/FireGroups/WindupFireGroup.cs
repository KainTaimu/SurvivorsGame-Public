// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class WindupFireGroup : AbstractFireGroup
{
	[Export]
	private AttackStats AttackStats { get; set; } = null!;

	[Export]
	private Curve _windupCurve = null!;

	private float _cooldown;
	private double _windupTime;

	private float WindupAttackSpeed =>
		Math.Clamp(_windupCurve.Sample((float)_windupTime), AttackStats.AttackSpeed, _windupCurve.MaxDomain);

	public override void ProcessInput()
	{
		var delta = (float)GetProcessDeltaTime();
		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
		{
			_windupTime = Math.Clamp(_windupTime - delta * 3, 0, _windupCurve.MaxDomain);
			return;
		}

		_cooldown -= delta;
		_windupTime = Math.Clamp(_windupTime + delta, 0, _windupCurve.MaxDomain);
		if (_cooldown > 0)
			return;

		EmitSignalOnFire();
		_cooldown = WindupAttackSpeed;
	}
}
