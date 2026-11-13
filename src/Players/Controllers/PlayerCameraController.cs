// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Game.Players.Controllers;

public partial class PlayerCameraController : SignalCamera2D
{
	[Export]
	public float MinZoom = 0.15f;

	[Export]
	public float MaxZoom = 0.9f;

	private float _shake;

	public override void _Process(double delta)
	{
		_shake = (float)Mathf.Max(0, _shake - delta);
		const float a = 15;
		const float b = 90;
		const float m = 0.1f;
		Rotation = Mathf.Pow(Mathf.E, -a * (3 - _shake)) * Mathf.Cos(b * (3 - _shake)) * m;
	}

	public override void _Input(InputEvent @event)
	{
		if (!@event.IsActionPressed("ZOOM_IN") && !@event.IsActionPressed("ZOOM_OUT"))
			return;

		var zoom = CurrentZoom;

		if (@event.IsActionPressed("ZOOM_IN"))
			zoom += 0.05f;
		else if (@event.IsActionPressed("ZOOM_OUT"))
			zoom -= 0.05f;

		CurrentZoom = float.Clamp(zoom, MinZoom, MaxZoom);
	}

	public void Shake()
	{
		_shake = 3;
	}
}
