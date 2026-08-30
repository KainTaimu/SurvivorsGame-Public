using Godot.Collections;

namespace Game.UI;

[GlobalClass]
public partial class PerformanceMonitor : CanvasLayer
{
	[Export]
	public Array<Node?>? Targets;

	[Export]
	public uint RefreshRate = 10;

	[ExportGroup("Internal")]
	[Export]
	private VBoxContainer _vBoxContainer = null!;

	[Export]
	private PackedScene _labelScene = null!;

	[Export]
	private Label? _fpsLabel;

	[Export]
	private RichTextLabel? _staticMemLabel;

	[Export]
	private RichTextLabel? _vidMemLabel;

	[Export]
	private RichTextLabel? _gcLabel;

	private readonly System.Collections.Generic.Dictionary<IFrameTimeTrackable, RichTextLabel> _labels = [];

	public override void _Ready()
	{
		Targets ??= [];

		foreach (var node in Targets)
		{
			if (node is null)
			{
				Logger.LogWarning("Null reference in Targets");
				continue;
			}

			if (node is not IFrameTimeTrackable)
			{
				Logger.LogError($"{node.Name} is not IFrameTimeTrackable");
				continue;
			}

			AddTarget(node);
		}

		if (OS.HasFeature("prod"))
			QueueFree();

#if !DEBUG
		_staticMemLabel?.QueueFree();
		_vidMemLabel?.QueueFree();
#endif

#if OS_WINDOWS
		_gcLabel?.QueueFree();
#endif
	}

	public override void _Process(double delta)
	{
		if (Engine.GetProcessFrames() % RefreshRate != 0)
			return;
		foreach (var (node, label) in _labels)
		{
			var unit = node.FrameTime.TimeUnit switch
			{
				FrameTimeUnitEnum.Ms => "ms",
				FrameTimeUnitEnum.Us => "µs",
				_ => throw new ArgumentOutOfRangeException(),
			};
			var time = node.FrameTime.TimeUnit switch
			{
				FrameTimeUnitEnum.Ms => node.FrameTime.ProcessTimeMicroSeconds * 1e-3,
				FrameTimeUnitEnum.Us => node.FrameTime.ProcessTimeMicroSeconds,
				_ => throw new ArgumentOutOfRangeException(),
			};
			label.Text = $"{node.FrameTime.FrameName}: {time:0.##}{unit}";
		}

#if DEBUG
		var staticMem = Performance.GetMonitor(Performance.Monitor.MemoryStatic);
		_staticMemLabel?.Text = $"Static Mem: {staticMem * 1e-6:F2}MB";
		var videoMem = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
		_vidMemLabel?.Text = $"Video Mem: {videoMem * 1e-6:F2}MB";
#endif

#if !OS_WINDOWS
		if (_gcLabel is not null)
		{
			var gcInfo = GC.GetGCMemoryInfo();
			_gcLabel.Text =
				$"GC Pause: {gcInfo.PauseTimePercentage:F1}%\nGC CommBytes: {gcInfo
					.TotalCommittedBytes * 1e-6:F2}MB";
		}
#endif
	}

	public void AddTarget(Node node)
	{
		if (node is not IFrameTimeTrackable trackable)
			return;
		var label = _labelScene.Instantiate<RichTextLabel>();
		_labels.Add(trackable, label);
		_vBoxContainer.AddChild(label);
	}

	public void RemoveTarget(Node node)
	{
		if (node is not IFrameTimeTrackable trackable)
			return;

		if (!_labels.Remove(trackable, out var value))
			return;

		value.QueueFree();
		node.QueueFree();
	}
}
