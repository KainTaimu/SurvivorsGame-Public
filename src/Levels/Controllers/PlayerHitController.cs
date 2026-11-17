// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics;
using Arch.Core;
using Game.Core;
using Game.Core.ECS;
using Game.Players;

namespace Game.Levels.Controllers;

public partial class PlayerHitController : Node
{
	[Export]
	private Player _player = null!;

	[Export]
	private AnimatedSprite2D _playerSprite = null!;

	[ExportCategory("")]
	[Export(PropertyHint.Range, "1,16, 1")]
	private int _maxEnemyContacts = 16;

	// ReSharper disable once ReturnTypeCanBeNotNullable
	private EnemyTargetQuery? TargetQuery => EnemyTargetQuery.Instance;

	private float PlayerHitboxRadius => _player.Character.CharacterStats.HitboxRadius;

	private CharacterStats PlayerStats => _player.Character.CharacterStats;

	private float _invisibilityTime;

	public override void _Process(double delta)
	{
		_invisibilityTime -= (float)delta;

		ProcessContacts();
	}

	private void ProcessContacts()
	{
		if (TargetQuery is null)
			return;

		if (_invisibilityTime > 0)
			return;

		Debug.Assert(_maxEnemyContacts <= Globals.MAX_STACKALLOC_COUNT);
		Span<Entity> entities = stackalloc Entity[_maxEnemyContacts];
		var hitCount = TargetQuery.TryGetTargetsInArea(
			_player.GlobalPosition,
			PlayerHitboxRadius,
			entities,
			_maxEnemyContacts,
			recordTime: false
		);
		if (hitCount <= 0)
			return;

		var damageSum = 0;

		for (var i = 0; i < hitCount; i++)
		{
			var entity = entities[i];
			if (!GameWorld.World.IsAlive(entity))
				continue;

			ref var damage = ref GameWorld.World.Get<EnemyContactDamageComponent>(entity);
			damageSum += damage.Damage;
		}

		if (damageSum <= 0)
			return;

		_invisibilityTime = PlayerStats.InvincibilityTime;
		PlayerStats.Damage(damageSum);

		DamageFeedback();
	}

	private void DamageFeedback()
	{
		if (_playerSprite.Material is not ShaderMaterial)
			return;

		var tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Expo);
		tween.SetEase(Tween.EaseType.Out);

		_playerSprite.SetInstanceShaderParameter("flash_state", 1f);

		tween.TweenMethod(
			Callable.From((float i) => _playerSprite.SetInstanceShaderParameter("flash_state", i)),
			1f,
			0f,
			1f
		);
	}
}
