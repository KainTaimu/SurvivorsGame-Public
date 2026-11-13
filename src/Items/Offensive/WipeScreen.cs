// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Arch.Core;
using Game.UI;

namespace Game.Items.Offensive;

public partial class WipeScreen : BaseOffensive, IHasAttackStats, IHasDamageStats
{
	[ExportCategory("Stats")]
	[Export]
	public DamageStats DamageStats { get; set; } = null!;

	[Export]
	public AttackStats AttackStats { get; set; } = null!;

	private Crosshair Crosshair => Crosshair.Instance!;

	private double _fireCooldown;

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;

		if (Input.IsActionPressed(InputMapNames.PrimaryAttack))
			Attack();
	}

	public void Attack()
	{
		if (_fireCooldown > 0)
			return;
		_fireCooldown = AttackStats.AttackSpeed;

		foreach (var entity in TargetQuery.GetTargetsInScreen())
			TryHandleHit(entity);
	}

	protected override void HandleDamage(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			DamageStats.Damage,
			DamageStats.CalculateCrit(),
			DamageStats.DamageVariance,
			PlayerStats.OutgoingDamage
		);
	}

	protected override void HandleHitEffects(Entity entity) { }
}
