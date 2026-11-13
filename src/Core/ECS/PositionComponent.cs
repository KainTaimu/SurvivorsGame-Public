// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Core.ECS;

public struct PositionComponent(Vector2 position)
{
	public Vector2 Position = position;

	public static PositionComponent operator +(PositionComponent left, PositionComponent right)
	{
		return new PositionComponent(left.Position + right.Position);
	}

	public static PositionComponent operator +(PositionComponent left, VelocityComponent right)
	{
		return new PositionComponent(left.Position + right.Velocity);
	}
}
