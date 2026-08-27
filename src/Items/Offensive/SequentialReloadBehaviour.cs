using Game.Levels.Controllers;

namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public partial class SequentialReloadBehaviour : AbstractReloadBehaviour
{
	[Export]
	private float _interruptPunishTime = 0.5f;

	[Export]
	private float _boltCloseTime = 0.2f;

	public float TimeBetweenRounds { get; set; }
	public int RoundsToLoad { get; set; }
	public int Step { get; set; } = 1;
	public bool IsInterrupted { get; private set; }

	private int _roundsLoaded;
	private float _timeUntilNextLoad;

	public override void Reload()
	{
		IsReloading = true;
		IsInterrupted = false;
		_roundsLoaded = 0;
		_timeUntilNextLoad = TimeBetweenRounds;
	}

	public override void Process(float delta)
	{
		if (!IsReloading)
			return;
		_timeUntilNextLoad -= delta;

		if (IsInterrupted)
			return;
		if (_timeUntilNextLoad > 0)
			return;

		_roundsLoaded += Step;
		_timeUntilNextLoad = TimeBetweenRounds;
		EmitSignalOnReloadProgress(_roundsLoaded, RoundsToLoad, Step);
		if (_roundsLoaded < RoundsToLoad)
			return;

		Reset();
		GameWorld.Instance.GetTree().CreateTimer(_boltCloseTime + TimeBetweenRounds, false).Timeout +=
			EmitSignalOnReloadEnd;
	}

	public void TryInterrupt()
	{
		IsInterrupted = true;
		GameWorld.Instance.GetTree().CreateTimer(_timeUntilNextLoad, false).Timeout += () =>
		{
			GameWorld.Instance.GetTree().CreateTimer(_interruptPunishTime, false).Timeout += () =>
			{
				GameWorld.Instance.GetTree().CreateTimer(_boltCloseTime, false).Timeout += () =>
				{
					Reset();
					EmitSignalOnReloadEndInterrupted();
				};
			};
		};
	}

	private void Reset()
	{
		IsReloading = false;
		IsInterrupted = false;
	}

	protected SequentialReloadBehaviour()
	{
		ResourceLocalToScene = true;
	}
}
