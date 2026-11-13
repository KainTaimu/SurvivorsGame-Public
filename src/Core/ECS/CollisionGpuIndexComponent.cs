// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Core.ECS;

// Dense slot (0..N-1) written by the GPU collision solver's upload pass
// and read by its apply pass. -1 = not participating this frame.
public record struct CollisionGpuIndexComponent(int Index)
{
	public static readonly CollisionGpuIndexComponent NotParticipating = new(-1);
}
