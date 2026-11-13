// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Levels.Controllers.Waves;

public partial class WaveDebugPanel : CanvasLayer
{
	[Export]
	private EnemyWaveController _waveController = null!;

	[ExportGroup("Internal")]
	[Export]
	private Label _timeLabel = null!;

	[Export]
	private Label? _waveInfo;

	[Export]
	private ProgressBar? _waveProgress;

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

	private void Update()
	{
		_timeLabel.Text = $"{TimeSpan.FromSeconds(Mathf.RoundToInt(_ticks)):g}";
		_waveInfo?.Text = OS.HasFeature("prod")
			? $"Wave {_waveController.CurrentWaveIndex + 1}"
			: _waveController.CurrentWave?.ToString();
		if (_waveController.CurrentWaveProgress is not null)
		{
			_waveProgress?.Visible = true;
			_waveProgress?.Value = (double)(1 - _waveController.CurrentWaveProgress);
		}
		else
			_waveProgress?.Visible = false;
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
			_isForceNextWaveBtnPressed = false;
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
