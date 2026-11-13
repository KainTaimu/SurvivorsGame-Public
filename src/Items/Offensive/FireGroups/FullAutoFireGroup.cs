// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Offensive;

[GlobalClass]
public partial class FullAutoFireGroup : AbstractFireGroup, ICooldown
{
	[Export]
	private AttackStats AttackStats { get; set; } = null!;

	private float CooldownDuration => AttackStats.AttackSpeed;
	private float _cooldown;

	public override void ProcessInput()
	{
		if (_cooldown > 0)
			return;

		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;

		_cooldown = CooldownDuration;
		EmitSignalOnFire();
	}

	public void Process(float delta)
	{
		_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
	}
}
