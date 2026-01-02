public partial class Test : MultiMeshInstance2D
{
    public override void _Ready()
    {
        // var quad = new QuadMesh { Size = new Vector2(32, 32) };
        // // Material that displays a texture on the quad
        // var mat = new CanvasItemMaterial();
        // // CanvasItemMaterial alone doesn’t “set a texture” like Sprite2D;
        // // commonly you’d use a ShaderMaterial to sample _texture.
        // // If you already have a working material/shader, assign it here:
        // Material = mat;
        // Multimesh = new MultiMesh
        // {
        //     Mesh = quad,
        //     TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
        //     UseCustomData = true,
        // };
        var mm = Multimesh;
        mm.InstanceCount = 1;

        for (int i = 0; i < mm.InstanceCount; i++)
        {
            var pos = new Vector2(GD.RandRange(0, 1920), GD.RandRange(0, 1080));
            var xform = new Transform2D(0.0f, pos);
            mm.SetInstanceTransform2D(i, xform);
            mm.SetInstanceCustomData(i, new Color(0, 0, 0, 1));
        }
    }

    public override void _Process(double delta)
    {
        var mm = Multimesh;
        for (int i = 0; i < mm.InstanceCount; i++)
        {
            mm.SetInstanceCustomData(i, new Color(0, 0, 0, GD.RandRange(0, 1)));
        }
    }
}
