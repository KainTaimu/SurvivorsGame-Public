using Godot.Collections;

namespace Game.UI;

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

	private readonly System.Collections.Generic.Dictionary<IFrameTimeTrackable, RichTextLabel> _labels = [];

	public override void _Ready()
	{
		if (Targets is null)
			return;

		foreach (var node in Targets)
		{
			if (node is null)
			{
				Logger.LogWarning("Null reference in Targets");
				continue;
			}

			if (node is not IFrameTimeTrackable trackable)
			{
				Logger.LogError($"{node.Name} is not IFrameTimeTrackable");
				continue;
			}

			var label = _labelScene.Instantiate<RichTextLabel>();

			_labels.Add(trackable, label);

			_vBoxContainer.AddChild(label);
		}
	}

	public override void _Process(double delta)
	{
		if (Engine.GetProcessFrames() % 30 != 0)
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
				FrameTimeUnitEnum.Ms => node.FrameTime.ProcessTime * 1e-3,
				FrameTimeUnitEnum.Us => node.FrameTime.ProcessTime,
				_ => throw new ArgumentOutOfRangeException(),
			};
			label.Text = $"{node.FrameTime.FrameName}: {time:0.##}{unit}";
		}
	}
}
