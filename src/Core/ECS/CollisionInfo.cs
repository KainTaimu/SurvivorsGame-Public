// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Core.ECS;

public enum CollisionLodLevel
{
	None,
	Near,
	Far,
}

public record struct CollisionLodComponent(CollisionLodLevel LodLevel);
