namespace Game.Levels.Controllers.Waves;

public partial class WaveDebugPanel : CanvasLayer
{
	[Export]
	private EnemyWaveController _waveController = null!;

	[ExportGroup("Internal")]
	[Export]
	private Label _timeLabel = null!;

	[Export]
	private Label _waveInfo = null!;

	[Export]
	private ProgressBar _waveProgress = null!;

	private double _ticks;
	private bool _isForceNextWaveBtnPressed;

	public override void _Ready()
	{
		CallDeferred(MethodName.Update);
	}

	public override void _PhysicsProcess(double delta)
	{
		_ticks += delta;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey)
			return;
		FastForward(Input.IsPhysicalKeyPressed(Key.Right));
		ForceNextWave();
		if (Input.IsPhysicalKeyPressed(Key.Delete))
		{
			var wave = _waveController.CurrentWave as IWaveResettable;
			wave?.Reset();
			CustomLogger.LogInfo("Wave reset");
		}
	}

	public void Update()
	{
		_timeLabel.Text = $"{TimeSpan.FromSeconds(Mathf.RoundToInt(_ticks)):g}";
		if (OS.HasFeature("prod"))
			_waveInfo.Text = $"Wave {_waveController.CurrentWaveIndex + 1}";
		else
			_waveInfo.Text = _waveController.CurrentWave?.ToString();
		_waveProgress.Value = 1 - _waveController.CurrentWaveProgress;
	}

	private void ForceNextWave()
	{
		if (Input.IsPhysicalKeyPressed(Key.Pageup))
		{
			if (_isForceNextWaveBtnPressed)
				return;
			if (!Input.IsPhysicalKeyPressed(Key.Shift))
				_waveController.CurrentWave?.Call(AbstractWave.MethodName.GiveRewards);
			_waveController.NextWave();
			_isForceNextWaveBtnPressed = true;
		}
		else
		{
			_isForceNextWaveBtnPressed = false;
		}
	}

	private void FastForward(bool flag)
	{
		var scale = 5;
		if (Input.IsPhysicalKeyPressed(Key.Shift))
			scale *= 2;
		if (Input.IsPhysicalKeyPressed(Key.Ctrl))
			scale *= 2;
		Engine.TimeScale = flag ? scale : 1;
	}
}
