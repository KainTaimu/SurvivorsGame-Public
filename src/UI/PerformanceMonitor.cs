// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using System.Text;
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
	private RichTextLabel? _frameTimeLabel;

	[Export]
	private RichTextLabel _staticMemLabel = null!;

	[Export]
	private RichTextLabel _vidMemLabel = null!;

	[Export]
	private RichTextLabel _gcLabel = null!;

	private readonly System.Collections.Generic.Dictionary<IFrameTimeTrackable, RichTextLabel> _labels = [];

	private long _lastAllocBytes;
	private long _lastAllocBytesTimeStamp;
	private double _lastAllocRate;
	private const double BYTES_PER_MEGABYTE = 1024.0 * 1024.0;

	public override void _Ready()
	{
		Targets ??= [];

		foreach (var node in Targets)
		{
			if (node is null)
			{
				CustomLogger.LogWarning("Null reference in Targets");
				continue;
			}

			if (node is not IFrameTimeTrackable)
			{
				CustomLogger.LogError($"{node.Name} is not IFrameTimeTrackable");
				continue;
			}

			AddTarget(node);
		}

		if (OS.HasFeature("prod"))
		{
			Hide();
		}

#if !DEBUG
		// These are unavailable in optimized builds
		_staticMemLabel.QueueFree();
		_vidMemLabel.QueueFree();
#endif

#if OS_WINDOWS
		_gcLabel.QueueFree();
#endif
	}

	private readonly StringBuilder _stringBuilder = new();

	public override void _Process(double delta)
	{
		if (!Visible)
			return;
		if (Engine.GetProcessFrames() % RefreshRate != 0)
			return;

		_frameTimeLabel?.Text = $"FrameTime: {Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000:F2}ms";

		var elapsed = Stopwatch.GetElapsedTime(_lastAllocBytesTimeStamp).TotalSeconds;
		var gcInfo = GC.GetGCMemoryInfo();
		if (elapsed > 1)
		{
			var thisAllocBytes = GC.GetTotalAllocatedBytes(true);
			var allocDelta = thisAllocBytes - _lastAllocBytes;
			_lastAllocRate = (allocDelta) / elapsed;
			_lastAllocBytes = thisAllocBytes;
			_lastAllocBytesTimeStamp = Stopwatch.GetTimestamp();

#if DEBUG
			var staticMem = Performance.GetMonitor(Performance.Monitor.MemoryStatic);
			_staticMemLabel.Text = $"StaticMem: {staticMem / BYTES_PER_MEGABYTE:F2}MiB";
			var videoMem = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);
			_vidMemLabel.Text = $"VideoMem: {videoMem / BYTES_PER_MEGABYTE:F2}MiB";
#endif
#if !OS_WINDOWS
			_gcLabel.Text =
				$"GC_Pause: {gcInfo.PauseTimePercentage:F1}%\nGC_CommBytes: {gcInfo
					.TotalCommittedBytes / BYTES_PER_MEGABYTE:F2}MiB\nGC_AllocRate: {_lastAllocRate / 1024:F2}KiB/s";
#endif
		}

		foreach (var (node, label) in _labels)
		{
			_stringBuilder.AppendLine(
				CreateString(node.FrameTime.FrameName, node.FrameTime.TimeUnit, node.FrameTime.ProcessTimeMicroSeconds)
			);
			if (node is IMultipleFrameTimeTrackable m)
			{
				foreach (var ft in m.FrameTimes)
					_stringBuilder.AppendLine(CreateString(ft.FrameName, ft.TimeUnit, ft.ProcessTimeMicroSeconds));
			}

			label.Text = _stringBuilder.ToString();
			_stringBuilder.Clear();
		}
	}

	private static string CreateString(string frameName, FrameTimeUnitEnum timeUnit, double processTime)
	{
		var unit = timeUnit switch
		{
			FrameTimeUnitEnum.Ms => "ms",
			FrameTimeUnitEnum.Us => "µs",
			_ => throw new ArgumentOutOfRangeException(),
		};
		var time = timeUnit switch
		{
			FrameTimeUnitEnum.Ms => processTime * 1e-3,
			FrameTimeUnitEnum.Us => processTime,
			_ => throw new ArgumentOutOfRangeException(),
		};
		return $"{frameName}: {time:0.##}{unit}";
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
