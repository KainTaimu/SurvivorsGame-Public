using System.Collections.Generic;
using Game.Core.ECS;
using Game.Core.Services;

namespace Game.Levels.Controllers;

public partial class EnemyRenderer : Node
{
    [Export]
    private EntityComponentStore _entities = null!;

    [Export]
    private PackedScene _multiMesh = null!;

    private readonly Dictionary<string, MultiMeshInstance2D> _spriteToMultiMesh = [];
    private readonly Dictionary<int, int> _idToInstanceIndex = [];

    public override void _Ready()
    {
        // var placeholderMmi = _multiMesh.Instantiate<MultiMeshInstance2D>();
        // AddChild(placeholderMmi);
        // _spriteToMultiMesh.Add("placeholder", placeholderMmi);
        // _placeholderMultiMesh = placeholderMmi;
    }

    public override void _Process(double delta)
    {
        foreach (
            var (id, sprite, pos) in _entities.Query<AnimatedSpriteComponent, PositionComponent>()
        )
        {
            if (!_spriteToMultiMesh.TryGetValue(sprite.SpriteName, out var mmi))
            {
                mmi = CreateNewMultiMesh(sprite.SpriteName);
                Logger.LogDebug("new mmi", mmi.Name);
            }
            if (!_idToInstanceIndex.TryGetValue(id, out var instanceIdx))
            {
                Logger.LogDebug("mmi index:", mmi.Multimesh.InstanceCount);
                _idToInstanceIndex.Add(id, mmi.Multimesh.InstanceCount);
                instanceIdx = mmi.Multimesh.InstanceCount;
                mmi.Multimesh.InstanceCount = mmi.Multimesh.InstanceCount + 1;
            }
            UpdateInstance(mmi, instanceIdx, pos.Position);
        }
    }

    private void UpdateInstance(MultiMeshInstance2D mmi, int instanceIdx, Vector2 newPos)
    {
        var multiMesh = mmi.Multimesh;
        multiMesh.SetInstanceTransform2D(instanceIdx, new Transform2D(0, newPos));
        multiMesh.SetInstanceCustomData(instanceIdx, new Color(0,0,1,1));
    }

    private MultiMeshInstance2D CreateNewMultiMesh(string spriteName)
    {
        var mmi = _multiMesh.Instantiate<MultiMeshInstance2D>();

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
}
