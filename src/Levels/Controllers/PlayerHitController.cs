// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Core.ECS;
using Game.Players;

namespace Game.Levels.Controllers;

public partial class PlayerHitController : Node
{
	[Export]
	private Player _player = null!;

	[Export]
	private AnimatedSprite2D _playerSprite = null!;

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

		if (!TargetQuery.TryGetTargetsInArea(_player.GlobalPosition, PlayerHitboxRadius, out var entities))
			return;

		var damageSum = 0;

		foreach (var entity in entities)
		{
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
