namespace Game.Core.Services;

[GlobalClass]
public partial class SpriteFrameMappingsService : Service
{
    [Export]
    private Godot.Collections.Dictionary<string, SpriteFrames> _spriteMapping = null!;

    [Export]
    public SpriteFrames PlaceholderSpriteFrame { get; private set; } = null!;

    public SpriteFrames GetSpriteFrame(string name)
    {
        if (!_spriteMapping.TryGetValue(name, out var sprite))
        {
            return PlaceholderSpriteFrame;
        }
        return sprite;
    }
}
