namespace Game.Levels.Controllers;

public partial class AirdropPlane : Node2D
{
	[Signal]
	public delegate void OnAirdropDroppedEventHandler(Vector2 dropPosition);

	public float Speed;

	private bool _isStarted;
	private float _timeToExpire;
	private float _t;
	private double _distanceToDrop;
	private double _displacement;
	private bool _hasDropped;
	private PackedScene? _dropScene;
	private Node? _dropParent;

	public override void _Process(double delta)
	{
		if (!_isStarted)
			return;

		_t += (float)delta;
		Position += Vector2.Right.Rotated(Rotation) * Speed * (float)delta;
		_displacement += Speed * delta;

		if (_displacement > _distanceToDrop && !_hasDropped)
			Drop();

		if (_t >= _timeToExpire)
		{
			QueueFree();
		}
	}

	public void Start(
		Vector2 startPosition,
		Vector2 dropPosition,
		float rotation,
		float speed,
		float timeToExpire,
		float dropPrecision = 2000,
		PackedScene? dropScene = null,
		Node? dropParent = null
	)
	{
		Rotation = rotation;
		GlobalPosition = startPosition;
		_distanceToDrop =
			startPosition.DistanceTo(dropPosition)
			+ GD.RandRange(dropPrecision / 2, dropPrecision)
				* GD.RandRange(-1, 1);
		Speed = speed;
		_isStarted = true;
		_timeToExpire = timeToExpire;
		_dropScene = dropScene;
		_dropParent = dropParent;
	}

	private void Drop()
	{
		_hasDropped = true;
		if (_dropScene is not null)
		{
			var drop = _dropScene.InstantiateOrNull<Node2D>();
			if (drop is null)
			{
				Logger.LogError(
					"Failed to instantiate drop. Does it inherit from Node2D?"
				);
				return;
			}
			drop.GlobalPosition = GlobalPosition;

			if (_dropParent is not null)
				_dropParent.CallDeferred(Node.MethodName.AddChild, drop);
			else
				GetTree().Root.CallDeferred(Node.MethodName.AddChild, drop);
		}

		Logger.LogInfo($"Dropped at {GlobalPosition}, {_t}s");
		EmitSignalOnAirdropDropped(GlobalPosition);
	}
}
