using System.Collections.Generic;
using System.Reflection;
using Game.Core.ECS;

namespace Game.Levels.Controllers;

internal readonly record struct DrawnRect(Rect2 Rect, bool Active);

public partial class EcsDebugVisualizer : Node2D
{
	[Export]
	private EntityComponentStore _componentStore = null!;

	[Export]
	private float _boxSize = 16;

	private readonly Dictionary<int, DrawnRect> _drawnRects = [];

	public override void _Ready()
	{
		_componentStore.OnEntityRegistered += HandleNewEntity;
		_componentStore.BeforeEntityUnregistered += HandleRemovedEntity;
	}

	public override void _Draw()
	{
		foreach (var (_, rect) in _drawnRects)
		{
			var color = new Color(255, 0, 0, 128);
			DrawRect(rect.Rect, rect.Active ? color : Colors.White);
		}
	}

	private void HandleNewEntity(int id)
	{
		var field = _componentStore
			.GetType()
			.GetField("_idToIndexTable", BindingFlags.NonPublic | BindingFlags.Instance);
		var idToIndexTable = (Dictionary<int, int>)field!.GetValue(_componentStore)!;

		var index = idToIndexTable[id];

		var x = (index % 16) * _boxSize;
		var y = (index / 16f) * _boxSize;
		var pos = new Vector2(x, y);
		var rect = new Rect2() { Size = Vector2.One * _boxSize, Position = pos };
		_drawnRects.Add(id, new DrawnRect(rect, true));
		QueueRedraw();
	}

	private void HandleRemovedEntity(int id)
	{
		_drawnRects.Remove(id);
		QueueRedraw();
	}
}
