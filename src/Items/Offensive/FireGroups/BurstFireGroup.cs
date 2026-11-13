// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Items.Offensive;

[GlobalClass]
public partial class BurstFireGroup : AbstractFireGroup, ICooldown
{
	[Export]
	private AttackStats AttackStats { get; set; } = null!;

	[Export]
	private BurstFireStats BurstFireStats { get; set; } = null!;

	private int BurstCount => BurstFireStats.BurstCount;
	private float TimeBetweenBursts => BurstFireStats.TimeBetweenBurst;
	private float CooldownDuration => AttackStats.AttackSpeed;

	private bool _isFireQueued;
	private float _cooldown;
	private int _shotsRemaining;

	// Allow queuing a shot if _cooldown is FIRE_QUEUE_TOLERANCE% of TimeBetweenBursts
	[Export]
	private float _fireQueueTolerance = 0.5f;

	public override void ProcessInput()
	{
		if (_shotsRemaining > 0)
			return;

		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return;

		if (_cooldown <= 0)
		{
			StartBurst();
			return;
		}

		var inQueueWindow = _cooldown <= TimeBetweenBursts * _fireQueueTolerance;
		if (inQueueWindow && !_isFireQueued)
			_isFireQueued = true;
	}

	private void StartBurst()
	{
		_shotsRemaining = BurstCount;
		_isFireQueued = false;
		_cooldown = 0;
	}

	public void Process(float delta)
	{
		if (_shotsRemaining > 0)
		{
			_cooldown = Mathf.Clamp(_cooldown - delta, 0, CooldownDuration);
			if (_cooldown > 0)
				return;

			_shotsRemaining--;
			_cooldown = _shotsRemaining > 0 ? CooldownDuration : TimeBetweenBursts;
			EmitSignalOnFire();
			return;
		}

		_cooldown = Mathf.Clamp(_cooldown - delta, 0, TimeBetweenBursts);
		if (_cooldown <= 0 && _isFireQueued)
			StartBurst();
	}
}
