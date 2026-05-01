using System.Collections.Generic;

namespace Game.VFX;

public partial class DamageIndicatorPool : Node2D
{
	[Export]
	public int PoolCount = 25;

	[Export]
	private PackedScene _indicatorScene = null!;

	private readonly Queue<DamageIndicator> _activePool = [];
	private readonly Queue<DamageIndicator> _inactivePool = [];

	public static DamageIndicatorPool? Instance;

	public override void _Ready()
	{
		Instance = this;
		for (var i = 0; i < PoolCount; i++)
		{
			var indicator = _indicatorScene.Instantiate<DamageIndicator>();
			indicator.OnFinished += ReturnIndicator;
			_inactivePool.Enqueue(indicator);
			AddChild(indicator);
			indicator.Hide();
		}
	}

	private void ReturnIndicator(DamageIndicator indicator)
	{
		_inactivePool.Enqueue(indicator);
		_activePool.Dequeue();
		indicator.ProcessMode = ProcessModeEnum.Disabled;
	}

	public void GetIndicator(Vector2 pos, int totalDamage, bool isCrit = false)
	{
		if (!_inactivePool.TryDequeue(out var indicator))
		{
			indicator = _activePool.Dequeue();
			indicator.Reset();
		}
		_activePool.Enqueue(indicator);
		indicator.ProcessMode = ProcessModeEnum.Inherit;
		indicator.ShowIndicator(pos, totalDamage, isCrit);
	}
}
