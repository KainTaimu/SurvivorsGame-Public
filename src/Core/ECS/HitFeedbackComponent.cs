// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Core.ECS;

public record struct HitFeedbackComponent()
{
	public required float HitTime
	{
		get;
		init
		{
			field = value;
			HitTimeLeft = value;
		}
	} = 0.5f;

	public float HitTimeLeft
	{
		get;
		set => field = Math.Clamp(value, 0f, float.MaxValue);
	}

	public int Damage;
	public bool IsCrit;
}
