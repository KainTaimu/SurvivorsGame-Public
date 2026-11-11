using Game.Levels.Controllers;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class SequentialReloadBehaviour : AbstractReloadBehaviour, IInterruptableReloadBehaviour
{
	[Export]
	private MagazineStats MagazineStats { get; set; } = null!;

	[Export]
	private ReloadStats ReloadStats { get; set; } = null!;

	[Export]
	private int Step { get; set; } = 1;

	[Export]
	private float _interruptPunishTime = 0.5f;

	[Export]
	private float _boltCloseTime = 0.2f;

	private float TimeBetweenRounds => ReloadStats.ReloadTime / MagazineStats.Capacity;
	private int RoundsToLoad { get; set; }
	private bool IsInterrupted { get; set; }

	private int _roundsLoaded;
	private float _timeUntilNextLoad;

	public override ReloadDisplayType DisplayType => ReloadDisplayType.Progressive;

	public override void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineStats.Count >= MagazineStats.Capacity)
			return;

		IsReloading = true;
		IsInterrupted = false;
		_roundsLoaded = 0;
		_timeUntilNextLoad = TimeBetweenRounds;
		RoundsToLoad = MagazineStats.Capacity - MagazineStats.Count;
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
		MagazineStats.Count++;
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
		if (!IsReloading)
			return;
		if (IsInterrupted)
			return;
		if (_roundsLoaded == 0)
			return;
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
}
