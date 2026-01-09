using System.Collections.Generic;
using Game.Core;
using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EntityRenderer : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    [Export]
    private PackedScene _multiMesh = null!;

    private readonly Dictionary<string, MultiMeshInstance2D> _spriteToMultiMesh = [];
    private readonly Dictionary<int, int> _idToInstanceIndex = [];

    private const int _initialInstanceCount = 2000;
    private const float _instanceGrowthMultiplier = 1.5f;

    public override void _Ready()
    {
        // Render last to allow other systems to do their work first
        ProcessPriority = 16;

        _entities.BeforeEntityUnregistered += BeforeEntityUnregistered;
    }

    public override void _Process(double delta)
    {
        foreach (
            var (id, sprite, pos, type) in _entities.Query<
                AnimatedSpriteComponent,
                PositionComponent,
                EntityTypeComponent
            >()
        )
        {
            if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
            {
                mmi = CreateNewMultiMesh(sprite.SpriteName);
            }
            if (!_idToInstanceIndex.TryGetValue(id, out var instanceIdx))
            {
                _idToInstanceIndex.Add(id, mmi.Multimesh.VisibleInstanceCount);
                instanceIdx = mmi.Multimesh.VisibleInstanceCount;
                mmi.Multimesh.VisibleInstanceCount = mmi.Multimesh.VisibleInstanceCount + 1;

                if (mmi.Multimesh.VisibleInstanceCount >= mmi.Multimesh.InstanceCount - 10)
                {
                    mmi.Multimesh.InstanceCount = (int)(
                        mmi.Multimesh.InstanceCount * _instanceGrowthMultiplier
                    );
                    Logger.LogDebug(mmi.Multimesh.InstanceCount);
                }
            }

            switch (type.EntityType)
            {
                case EntityType.Enemy:
                    UpdateEnemyInstance(mmi, instanceIdx, pos.Position);
                    break;
            }
        }
    }

    private MultiMeshInstance2D CreateNewMultiMesh(string spriteName)
    {
        var mmi = _multiMesh.Instantiate<MultiMeshInstance2D>();
        // To avoid flickering, pre-initialize 1,000 instances and only make them visible when spawned
        mmi.Multimesh.InstanceCount = _initialInstanceCount;
        mmi.Multimesh.VisibleInstanceCount = 0;

        var ss = ServiceLocator.GetService<SpriteFrameMappingsService>();
        if (ss is null)
        {
            Logger.LogError("Could not get SpriteFrameMappingsService.");
            mmi.Texture = new PlaceholderTexture2D();
            return mmi;
        }

        mmi.Texture = ss.GetSpriteFrame(spriteName);

        AddChild(mmi);
        _spriteToMultiMesh.Add(spriteName, mmi);
        return mmi;
    }

    // NOTE: See NOTES.md for shader data layout
    private void UpdateEnemyInstance(MultiMeshInstance2D mmi, int instanceIdx, Vector2 newPos)
    {
        int flip;
        var player = GameWorld.Instance.MainPlayer;
        if (player is null)
            flip = 0;
        else
            flip = player.GlobalPosition > newPos ? 0 : 1;

        var multiMesh = mmi.Multimesh;
        multiMesh.SetInstanceTransform2D(instanceIdx, new Transform2D(0, newPos));

        var data = new Color(0, 0, flip, 1);
        multiMesh.SetInstanceCustomData(instanceIdx, data);
    }

    private void BeforeEntityUnregistered(int id)
    {
        if (!_entities.GetComponent<AnimatedSpriteComponent>(id, out var sprite))
        {
            Logger.LogDebug("sprite not found");
            return;
        }
        if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
        {
            Logger.LogDebug("multiMesh not found");
            return;
        }

        if (!_idToInstanceIndex.TryGetValue(id, out var idx))
        {
            Logger.LogDebug("instanceIdx not found");
            return;
        }

        var multiMesh = mmi.Multimesh;
        multiMesh.SetInstanceTransform2D(idx, new Transform2D(0, Vector2.Inf)); // HACK: Hide the instance in narnia

        _idToInstanceIndex.Remove(id);
    }
}
