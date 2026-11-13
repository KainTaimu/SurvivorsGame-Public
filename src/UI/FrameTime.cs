using System.Diagnostics;

namespace Game.UI;

[GlobalClass]
public partial class FrameTime : Resource
{
	[Export]
	public StringName FrameName = null!;

	[Export]
	public FrameTimeUnitEnum TimeUnit;

	public double ProcessTimeMicroSeconds;

	public IDisposable Record()
	{
		var stopwatch = Stopwatch.StartNew();
		return new DelegateDisposable(() =>
		{
			ProcessTimeMicroSeconds = stopwatch.Elapsed.TotalMicroseconds;
		});
	}

	private sealed class DelegateDisposable(Action onDispose) : IDisposable
	{
		public void Dispose()
		{
			onDispose();
		}
	}
}

public interface IFrameTimeTrackable
{
	FrameTime FrameTime { get; }
}

public enum FrameTimeUnitEnum
{
	Ms,
	Us,
}
