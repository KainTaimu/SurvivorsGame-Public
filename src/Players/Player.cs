// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Players.Controllers;

namespace Game.Players;

[GlobalClass]
public partial class Player : Node2D
{
	[Signal]
	public delegate void OnHealthChangedEventHandler(int newHealth, int difference);

	[Obsolete("Use OnHealthChanged signal")]
	[Signal]
	public delegate void OnDamagedEventHandler(int damage);

	[Signal]
	public delegate void OnPlayerDeathEventHandler();

	[Export]
	public Character Character { get; private set; } = null!;

	[Export]
	public PlayerMovementController MovementController { get; private set; } = null!;

	[Export]
	public AbstractPlayerWeaponController WeaponController { get; private set; } = null!;

	[Export]
	public PlayerStatusEffectController StatusEffectController { get; private set; } = null!;

	public bool IsAlive => Character.CharacterStats.Health > 0;

	public override void _Ready()
	{
		if (!HasConnections(SignalName.OnPlayerDeath))
			CustomLogger.LogWarning("No connections to OnDamaged signal");
	}

	public void OnDeath()
	{
		ProcessMode = ProcessModeEnum.Disabled;
	}
}
