// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Offensive;

[GlobalClass]
public partial class SemiAutoFireGroup : AbstractFireGroup, ICooldown
{
	[Export]
	private AttackStats AttackStats { get; set; } = null!;

	private float CooldownDuration => AttackStats.AttackSpeed;

	private bool _isFireQueued;
	private ulong _ticksSinceLastFire;
	private float _cooldown;

	// Allow queuing a shot if _cooldown is FIRE_QUEUE_TOLERANCE% of CooldownDuration
	[Export]
	private float _fireQueueTolerance = 0.5f;

	public override void ProcessInput()
	{
		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return;

		if (_cooldown <= 0)
		{
			ResetOnFire();
			EmitSignalOnFire();
			return;
		}

		var inQueueWindow = _cooldown <= CooldownDuration * _fireQueueTolerance;
		if (inQueueWindow && !_isFireQueued)
			_isFireQueued = true;
	}

	private void ResetOnFire()
	{
		_cooldown = CooldownDuration;
		_isFireQueued = false;
		_ticksSinceLastFire = Time.GetTicksMsec();
	}

	public void Process(float delta)
	{
		_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
		if (_cooldown <= 0 && _isFireQueued)
		{
			ResetOnFire();
			EmitSignalOnFire();
		}
	}
}
