// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Items.Offensive;
using Game.Levels.Controllers;
using Game.Players;

namespace Game.UI;

public partial class MuzzleFlash : Node
{
	[Export]
	public BaseOffensive Offensive = null!;

	[Export]
	public PointLight2D Light = null!;

	private Player Player => GameWorld.Instance.MainPlayer;
	private Crosshair Crosshair => Crosshair.Instance!;

	public override void _Ready()
	{
		Offensive.OnAttack += () =>
		{
			Light.GlobalPosition = Player.GlobalPosition;
			Light.GlobalRotation =
				Light.GlobalPosition.AngleToPoint(Crosshair.GlobalSpacePosition) + 90 * Mathf.Pi / 180;
			Light.Show();
			GetTree().CreateTimer(0.05, false).Timeout += () => Light.Hide();
		};
	}
}
