// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Arch.Core;

namespace Game.Items.Offensive;

public partial class DamageInjector : AbstractInjector
{
	[Export]
	private bool RemoveAfterInjection { get; set; }

	public override void Process(double delta)
	{
		if (!Input.IsActionJustPressed(InputMapNames.PrimaryAttack))
			return;
		Inject();
	}

	public void Inject()
	{
		EmitSignalOnAttack();
		if (RemoveAfterInjection)
			Callable.From(QueueFree).CallDeferred();
	}

	protected override void HandleDamage(Entity entity) { }

	protected override void HandleHitEffects(Entity entity) { }
}
