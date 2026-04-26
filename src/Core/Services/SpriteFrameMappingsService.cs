using Game.Levels.Controllers;

namespace Game.Core.Services;

[GlobalClass]
public partial class SpriteFrameMappingsService : Service
{
	[Export]
	private Godot.Collections.Dictionary<
		string,
		FodderEnemySpriteInfo
	> _spriteMapping = null!;

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
			Logger.LogError($"{name} does not exist");
			return PlaceholderSpriteFrame;
		}
		return sprite.SpriteFrames;
	}
}
