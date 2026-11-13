// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Players;
using Game.UI;

namespace Game.Levels.Controllers;

public partial class AirdropManager : Node2D
{
	[Export]
	private PackedScene _airdropPlaneScene = null!;

	[Export]
	private PackedScene _itemDropScene = null!;

	[Export]
	public float PlaneVelocity = 3000;

	private Player Player => GameWorld.Instance.MainPlayer;
	private Vector2 DropPosition => Crosshair.Instance!.GlobalSpacePosition;

	private bool _f1Held;

	private static readonly string _bombKey = "CARPET_BOMB_CHEAT";

	public override void _Process(double delta)
	{
		var prev = _f1Held;
		if (Input.IsActionJustPressed(_bombKey))
			_f1Held = true;
		else
		{
			_f1Held = false;
			return;
		}

		if (prev != _f1Held)
			DeployItemAirdrop(DropPosition, GD.RandRange(0, 360), 4);
	}

	public void DeployItemAirdrop(Vector2 dropPosition, float arrivalAngleDeg, float timeToArrivalSec)
	{
		DeployAirdrop(dropPosition, arrivalAngleDeg, timeToArrivalSec, _itemDropScene, this);
	}

	private void DeployAirdrop(
		Vector2 dropPosition,
		float arrivalAngleDeg,
		float timeToArrivalSec,
		PackedScene? dropScene = null,
		Node? dropParent = null
	)
	{
		var distance = PlaneVelocity * timeToArrivalSec;
		var rotVec = Vector2.One.Rotated(Mathf.DegToRad(arrivalAngleDeg));
		rotVec *= distance;

		var rand = new Vector2((float)GD.RandRange(-1f, 1f), (float)GD.RandRange(-1f, 1f));
		rand *= 1920 / 2f;
		var startPosition = dropPosition + rotVec;

		var aircraft = _airdropPlaneScene.Instantiate<AirdropPlane>();
		AddChild(aircraft);
		aircraft.Start(
			startPosition,
			dropPosition,
			startPosition.AngleToPoint(dropPosition + rand),
			PlaneVelocity,
			timeToArrivalSec * 2,
			0,
			dropScene,
			dropParent
		);
		CustomLogger.LogInfo(
			$"Spawned airdrop : Drop={dropPosition} : TTA={timeToArrivalSec} : Angle={arrivalAngleDeg}"
		);
	}
}
