// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Offensive;

[GlobalClass]
public partial class DamageStats : ItemStat
{
	[Export]
	public int Damage { get; set; } = 4;

	[Export]
	public float DamageVariance { get; set; } = 0.15f;

	[Export]
	public float CritDamageMultiplier { get; set; } = 1.5f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float CritDamageChance { get; set; } = 0.1f;

	public int CalculateCrit()
	{
		var roll = GD.Randf();
		if (roll > CritDamageChance)
			return 0;

		return Mathf.CeilToInt(Damage * CritDamageMultiplier);
	}
}
