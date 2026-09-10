namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class WaveBreak : AbstractWave
{
	[Export]
	public float TimeToEnd;

	private float _timeToEnd;
	private bool _started;

	public override void Process(double delta)
	{
		if (!_started)
			return;

		_timeToEnd -= (float)delta;
		if (_timeToEnd <= 0)
			EndWave();
	}

	public override void Initialize(EnemyWaveController waveController) { }

	public override void StartWave(int waveIndex)
	{
		EmitSignalOnWaveStart();
		_timeToEnd = TimeToEnd;
		_started = true;
	}

	public override void EndWave()
	{
		_started = false;
		GiveRewards();
		EmitSignalOnWaveEnd();
	}

	public override void SpawnEnemy() { }

	public override string ToString()
	{
		return "....."[..Mathf.CeilToInt(TimeToEnd - _timeToEnd)];
	}
}