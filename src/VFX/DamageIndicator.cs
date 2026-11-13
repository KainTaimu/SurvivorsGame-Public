// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.VFX;

// NOTE: Tightly coupled with DamageIndicatorPool
public partial class DamageIndicator : Node2D
{
	[Signal]
	public delegate void OnFinishedEventHandler(DamageIndicator indicator);

	private Vector2 _finalPosition;

	[Export]
	private Label _label = null!;

	private Tween? _tween;

	public void ShowIndicator(Vector2 pos, int damage, bool isCrit = false)
	{
		Show();
		const float variation = 10;
		var randomOffsetX = (float)GD.RandRange(-variation, variation);
		var randomOffsetY = (float)GD.RandRange(-variation, variation);

		Position = new Vector2(pos.X + randomOffsetX, pos.Y + randomOffsetY - 55);
		_label.Text = damage.ToString();
		_label.LabelSettings.FontColor = isCrit ? Colors.OrangeRed : Colors.White;

		_finalPosition = new Vector2(Position.X + 15, Position.Y - 15);

		_tween?.Kill();
		_tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_tween.Parallel().TweenProperty(this, "position", _finalPosition, 1.75f);
		_tween.Parallel().TweenProperty(this, "modulate", Colors.Transparent, 1.75f).SetDelay(0.2f);
		_tween.TweenCallback(
			Callable.From(() =>
			{
				Reset();
				EmitSignalOnFinished(this);
			})
		);
	}

	public void Reset()
	{
		_tween?.Kill();
		_tween = null;
		Hide();
		Modulate = Colors.White;
	}
}
