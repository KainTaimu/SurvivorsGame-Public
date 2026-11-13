// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Models;

[Tool]
[GlobalClass]
public partial class FloatStat : AbstractStat
{
	[Export]
	public float BaseValue = 1f;

	public float Value => (BaseValue + GetFlatSum()) * GetMultipliersSum();

	public override string ToString()
	{
		return $"{Value} ({BaseValue} + {GetFlatSum():F2} * {GetMultipliersSum():F2})";
	}
}
