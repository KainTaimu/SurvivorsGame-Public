namespace SurvivorsGame.ui;

public partial class FpsCounter : CanvasLayer
{
    [Export]
    private Label _fps;

    [Export]
    private Label _processTime;

    [Export]
    private Label _physicsTime;

    public override void _Process(double delta)
    {
        var fps = Performance.GetMonitor(Performance.Monitor.TimeFps);
        var processTime = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000;
        var physicsTime = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000;

        _fps.Text = $"FPS: {fps:F2}";
        _processTime.Text = $"Process: {processTime:F2}ms";
        _physicsTime.Text = $"Physics: {physicsTime:F2}ms";
    }
}
