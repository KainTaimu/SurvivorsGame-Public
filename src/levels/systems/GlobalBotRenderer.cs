using System.Collections.Generic;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class GlobalBotRenderer : Node2D
{
    private static readonly StringName DefaultString = "default"; // stop implicit conversion in draw method

    public static GlobalBotRenderer Instance;

    public GlobalBotRenderer()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
    }

    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        var entityMan = GlobalEntityManager.Instance;
        foreach (var id in entityMan.GetIds())
        {
            var sprite = entityMan.GetSprite(id);
            var position = entityMan.GetPosition(id);
            DrawTexture(sprite.GetFrameTexture(DefaultString, 0), position);
        }
    }

    private struct BotRenderCache
    {
        public int FrameCount { get; }

        public readonly Dictionary<int, Texture2D> SpriteTextures = [];

        public BotRenderCache(AnimatedSprite2D sprite)
        {
            var spriteFrames = sprite.GetSpriteFrames();
            var frameCount = spriteFrames.GetFrameCount(DefaultString);
            for (var i = 0; i < frameCount; i++)
            {
                SpriteTextures.Add(i, spriteFrames.GetFrameTexture(DefaultString, i));
            }

            FrameCount = frameCount;
        }
    }
}
