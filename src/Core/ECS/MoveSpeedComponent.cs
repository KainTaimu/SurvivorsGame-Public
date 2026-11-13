// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Core.ECS;

public record struct MoveSpeedComponent(float MoveSpeed, float TurnSpeed = 5);
