// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class HitParticlesInfo : Resource
{
	[Export]
	public int Amount;

	[Export]
	public ParticleProcessMaterial ProcessMaterial = null!;
}
