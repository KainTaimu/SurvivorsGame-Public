// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
