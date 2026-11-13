// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Game.Core.Settings;

namespace Game.VFX;

public partial class DamageIndicatorPool : Node2D
{
	[Export]
	public int PoolCount = 25;

	[Export]
	private PackedScene _indicatorScene = null!;

	[Export]
	private CanvasLayer _indicatorsLayer = null!;

	private readonly Queue<DamageIndicator> _activePool = [];
	private readonly Queue<DamageIndicator> _inactivePool = [];

	public static DamageIndicatorPool? Instance;

	public bool Enabled => GameSettings.Instance.EnableDamageIndicators;

	public override void _Ready()
	{
		GameSettings.Instance.OnDamageIndicatorsChanged += () =>
		{
			ProcessMode = Enabled ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
			if (Enabled)
				PopulatePool();
			else
			{
				while (_activePool.Count > 0)
					_activePool.Dequeue().QueueFree();
				while (_inactivePool.Count > 0)
					_inactivePool.Dequeue().QueueFree();
			}
		};

		Instance = this;
		if (Enabled)
			PopulatePool();
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	private void PopulatePool()
	{
		for (var i = 0; i < PoolCount; i++)
		{
			var indicator = _indicatorScene.Instantiate<DamageIndicator>();
			indicator.OnFinished += ReturnIndicator;
			_inactivePool.Enqueue(indicator);
			_indicatorsLayer.AddChild(indicator);
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
