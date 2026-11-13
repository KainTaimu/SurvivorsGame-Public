// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Levels.Controllers;

namespace Game.Core.ECS;

public readonly record struct EnemyBehaviorComponent(EnemyBehaviorTypes Type = EnemyBehaviorTypes.None);
