using System.Collections.Generic;
using SurvivorsGame.Systems;

namespace SurvivorsGame.Levels.Systems;

public partial class GlobalBotRenderer : Node2D
{
    private static readonly StringName DefaultString = "default"; // stop implicit conversion in draw method

    private static readonly Dictionary<Rid, Vector2> TextureOffsetCache = [];

    private static readonly Dictionary<StringName, BotRenderCache> RenderCaches = [];

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
        foreach (var enemyList in GameWorld.Instance.EnemiesByType.Values)
        {
            foreach (var enemy in enemyList)
            {
                var spriteNode = enemy.Sprite;
                if (!RenderCaches.TryGetValue(enemy.GetSceneFilePath(), out var renderCache))
                {
                    RenderCaches.Add(enemy.GetSceneFilePath(), new BotRenderCache(spriteNode));
                    return;
                }

                var pairs = renderCache.FrameCount / 2;
                var baseFrame = spriteNode.Frame % pairs;
                var facingOffset = spriteNode.FlipH ? 1 : 0; // 0 = right, 1 = left
                var frameIndex = baseFrame * 2 + facingOffset;
                var tex = renderCache.SpriteTextures[frameIndex];
                var texRid = tex.GetRid();

                if (!TextureOffsetCache.TryGetValue(texRid, out var offset))
                {
                    offset = -tex.GetSize() * 0.5f;
                    TextureOffsetCache.Add(texRid, offset);
                }

                DrawTexture(tex, enemy.Position + offset);
            }
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