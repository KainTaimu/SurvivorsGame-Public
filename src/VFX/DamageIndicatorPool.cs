using System.Collections.Generic;
using Game.Core.Settings;

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

	public bool Enabled => GameSettings.Instance.EnableDamageIndicators;

	public override void _Ready()
	{
		GameSettings.Instance.OnDamageIndicatorsChanged += () =>
		{
			ProcessMode = Enabled
				? ProcessModeEnum.Inherit
				: ProcessModeEnum.Disabled;
			if (Enabled)
			{
				PopulatePool();
			}
			else
			{
				while (_activePool.Count > 0)
				{
					_activePool.Dequeue().QueueFree();
				}
				while (_inactivePool.Count > 0)
				{
					_inactivePool.Dequeue().QueueFree();
				}
			}
		};

		Instance = this;
		if (Enabled)
		{
			PopulatePool();
		}
	}

	private void PopulatePool()
	{
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
		if (!Enabled)
			return;
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
