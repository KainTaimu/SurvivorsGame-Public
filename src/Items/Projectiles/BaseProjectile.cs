// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Game.Items.Projectiles;

public abstract partial class BaseProjectile : Node2D, IProjectile
{
	[Signal]
	public delegate void OnEntityHitEventHandler(EntityObject entityObject);

	public Node? Origin { get; private set; }
	public float HitRadius { get; private set; } = 24f;
	public float ProjectileSpeed { get; private set; }
	public int PierceLimit { get; private set; }

	protected bool IsInitialized { get; set; }

	public void Initialize(Node? origin, float hitRadius, float speed, int pierceLimit)
	{
		Origin = origin;
		HitRadius = hitRadius;
		ProjectileSpeed = speed;
		PierceLimit = pierceLimit;
		IsInitialized = true;
		PostInitialization();
	}

	protected abstract void PostInitialization();
}
