namespace Game.Utils;

public class DebugRectCreator(SceneTree tree)
{
    private readonly SceneTree _tree = tree;

    public ReferenceRect CreateRect(Vector2 pos, Vector2 size)
    {
        var rect = new ReferenceRect
        {
            Size = size,
            Position = pos,
            Visible = true,
            EditorOnly = false,
        };

        _tree.Root.AddChild(rect);
        return rect;
    }
}
