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
			if (!GameWorld.World.TryGet<EnemyContactDamageComponent>(entity, out var damage))
				continue;
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
		if (_playerSprite.Material is not ShaderMaterial spriteShaderMaterial)
			return;

		var tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Expo);
		tween.SetEase(Tween.EaseType.Out);

		spriteShaderMaterial.SetShaderParameter("flash_state", 1f);
		spriteShaderMaterial.SetShaderParameter("color", Colors.White);

		tween.TweenMethod(
			Callable.From((float i) => spriteShaderMaterial.SetShaderParameter("flash_state", i)),
			1f,
			0f,
			1f
		);
	}
}
