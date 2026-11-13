// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Core.ECS;

public enum DeathCauseEnum
{
	Normal,
	Explosion,
	LargeDeath,
}

public readonly record struct DeathCauseComponent(DeathCauseEnum CauseEnum);
