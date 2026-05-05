using Game.Players;

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
	private Vector2 PlayerPosition => Player.GlobalPosition;

	public void DeployItemAirdrop(
		Vector2 dropPosition,
		float arrivalAngleDeg,
		float timeToArrivalSec
	)
	{
		DeployAirdrop(
			dropPosition,
			arrivalAngleDeg,
			timeToArrivalSec,
			_itemDropScene,
			this
		);
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
		var rotVec = Vector2.One.Rotated(
			(arrivalAngleDeg - 180) * Mathf.Pi / 180
		);
		rotVec *= distance;

		var rand = new Vector2(
			(float)GD.RandRange(-1f, 1f),
			(float)GD.RandRange(-1f, 1f)
		);
		rand *= 1920 / 2;
		var startPosition = dropPosition + rotVec;

		var aircraft = _airdropPlaneScene.Instantiate<AirdropPlane>();
		AddChild(aircraft);
		aircraft.Start(
			startPosition,
			dropPosition,
			startPosition.AngleToPoint(dropPosition + rand),
			PlaneVelocity,
			timeToArrivalSec * 2,
			dropScene: dropScene,
			dropParent: dropParent
		);
	}
}
