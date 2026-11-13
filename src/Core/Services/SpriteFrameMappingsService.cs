// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Levels.Controllers;
using Godot.Collections;

namespace Game.Core.Services;

[GlobalClass]
public partial class SpriteFrameMappingsService : Service
{
	[Export]
	private Dictionary<string, FodderEnemySpriteInfo> _spriteMapping = null!;

	[Export]
	public Texture2D PlaceholderSpriteFrame { get; private set; } = null!;

	public FodderEnemySpriteInfo? GetSpriteInfo(string name)
	{
		if (!_spriteMapping.TryGetValue(name, out var sprite))
			return null;
		return sprite;
	}

	public Texture2D GetSpriteFrame(string name)
	{
		if (!_spriteMapping.TryGetValue(name, out var sprite))
		{
			CustomLogger.LogError($"{name} does not exist");
			return PlaceholderSpriteFrame;
		}

		return sprite.SpriteFrames;
	}
}
