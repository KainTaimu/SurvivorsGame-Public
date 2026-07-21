namespace Game.UI;

[GlobalClass]
public partial class FrameTime : Resource
{
	[Export]
	public StringName FrameName = null!;

	[Export]
	public FrameTimeUnitEnum TimeUnit;

	public double ProcessTime;

	public IDisposable Record()
	{
		var start = Time.GetTicksUsec();
		return new DelegateDisposable(() =>
		{
			ProcessTime = Time.GetTicksUsec() - start;
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
