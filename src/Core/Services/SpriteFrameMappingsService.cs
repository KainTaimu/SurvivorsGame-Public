namespace Game.Core.Services;

[GlobalClass]
public partial class SpriteFrameMappingsService : Service
{
    [Export]
    private Godot.Collections.Dictionary<string, Texture2D> _spriteMapping = null!;

    [Export]
    public Texture2D PlaceholderSpriteFrame { get; private set; } = null!;

    public Texture2D GetSpriteFrame(string name)
    {
        if (!_spriteMapping.TryGetValue(name, out var sprite))
        {
            return PlaceholderSpriteFrame;
        }
        return sprite;
    }
}
