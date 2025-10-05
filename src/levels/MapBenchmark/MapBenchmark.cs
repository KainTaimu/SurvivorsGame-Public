namespace SurvivorsGame.Levels;

public partial class MapBenchmark : BaseMap
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Logger.LogDebug($"Map center: {PixelSize * 0.5f}");
    }
}