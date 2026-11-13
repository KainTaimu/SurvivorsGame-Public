// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;

namespace Game.Levels.Controllers;

[GlobalClass]
public abstract partial class AbstractEnemyBehavior : Resource
{
	public abstract void AddComponent(Entity entity);
}
