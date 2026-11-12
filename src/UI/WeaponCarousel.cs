using System.Collections.Generic;

namespace Game.UI;

public partial class WeaponCarousel : Control
{
	[Export]
	public float ItemSpacing { get; set; } = 20.0f;

	[Export]
	public float SlideDuration { get; set; } = 0.2f;

	[Export]
	public bool WrapAround { get; set; } = true;

	[Export]
	public float UnselectedAlpha { get; set; } = 0.5f;

	[Signal]
	public delegate void SelectionChangedEventHandler(int index);

	[Export]
	private Control _track = null!;

	private readonly List<Control> _items = [];
	private int _selectedIndex;
	private bool _hasInitialSelection;
	private Tween? _tween;

	public int ItemCount => _items.Count;

	public override void _Ready()
	{
		ClipContents = true;

		foreach (var child in _track.GetChildren())
		{
			if (child is Control control)
			{
				_items.Add(control);
			}
		}

		LayoutItems();
		SnapToIndex(_selectedIndex);
	}

	public T EnsureItemCount<T>(int count, PackedScene itemScene)
		where T : Control
	{
		var diff = count - _items.Count;
		while (diff > 0)
		{
			var item = itemScene.Instantiate<T>();
			_track.AddChild(item);
			_items.Add(item);
			diff--;
		}

		while (diff < 0)
		{
			var lastIndex = _items.Count - 1;
			var item = _items[lastIndex];
			_items.RemoveAt(lastIndex);
			_track.RemoveChild(item);
			item.QueueFree();
			diff++;
		}

		if (_items.Count == 0)
		{
			_selectedIndex = 0;
			_hasInitialSelection = false;
		}
		else if (_selectedIndex >= _items.Count)
		{
			_selectedIndex = _items.Count - 1;
		}

		return _items.Count > 0 ? (T)_items[0] : null!;
	}

	public Control GetItem(int index) => _items[index];

	public void RefreshLayout()
	{
		LayoutItems();
	}

	public void SelectIndex(int newIndex, bool animate = true)
	{
		var count = _items.Count;
		if (count == 0)
			return;

		var nextIndex = WrapAround ? Mathf.PosMod(newIndex, count) : Mathf.Clamp(newIndex, 0, count - 1);

		if (!_hasInitialSelection || !animate)
		{
			_hasInitialSelection = true;
			var changed = nextIndex != _selectedIndex;
			_selectedIndex = nextIndex;
			SnapToIndex(_selectedIndex);
			if (changed)
				EmitSignal(SignalName.SelectionChanged, _selectedIndex);
			return;
		}

		if (nextIndex == _selectedIndex)
		{
			AnimateToIndex(_selectedIndex);
			return;
		}

		_selectedIndex = nextIndex;
		AnimateToIndex(_selectedIndex);
		EmitSignal(SignalName.SelectionChanged, _selectedIndex);
	}

	private void LayoutItems()
	{
		var cursorX = 0.0f;
		foreach (var item in _items)
		{
			item.ResetSize();
			var itemSize = item.GetCombinedMinimumSize();
			item.Size = itemSize;
			item.PivotOffset = itemSize / 2.0f;
			item.Position = new Vector2(cursorX, (Size.Y - itemSize.Y) / 2.0f);
			cursorX += itemSize.X + ItemSpacing;
		}
	}

	private float GetTargetTrackX(int index)
	{
		var targetItem = _items[index];
		var viewportCenterX = Size.X / 2.0f;
		var itemCenterX = targetItem.Position.X + targetItem.Size.X / 2.0f;
		return viewportCenterX - itemCenterX;
	}

	private void SnapToIndex(int index)
	{
		if (_items.Count == 0)
			return;

		if (_tween != null && _tween.IsRunning())
			_tween.Kill();

		_track.Position = new Vector2(GetTargetTrackX(index), _track.Position.Y);

		for (var i = 0; i < _items.Count; i++)
		{
			var isSelected = i == index;

			var modulate = _items[i].Modulate;
			modulate.A = isSelected ? 1.0f : UnselectedAlpha;
			_items[i].Modulate = modulate;
		}
	}

	private static readonly NodePath _positionX = "position:x";
	private static readonly NodePath _modulateA = "modulate:a";

	private void AnimateToIndex(int index)
	{
		if (_items.Count == 0)
			return;

		if (_tween != null && _tween.IsRunning())
			_tween.Kill();

		_tween = CreateTween().SetParallel();

		var targetX = GetTargetTrackX(index);
		if (Mathf.Abs(targetX - _track.Position.X) < 300)
			_tween.SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
		else
			_tween.SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);

		_tween.TweenProperty(_track, _positionX, targetX, SlideDuration);

		for (var i = 0; i < _items.Count; i++)
		{
			var isSelected = i == index;
			var targetAlpha = isSelected ? 1.0f : UnselectedAlpha;

			_tween.TweenProperty(_items[i], _modulateA, targetAlpha, SlideDuration);
		}
	}
}
