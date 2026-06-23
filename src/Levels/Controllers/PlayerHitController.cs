using Arch.Core;
using Game.Core.ECS;
using Game.Players;

namespace Game.Levels.Controllers;

public partial class PlayerHitController : Node
{
	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;

	private Player Player => GameWorld.Instance.MainPlayer;

	private float PlayerHitboxRadius => Player.Character.CharacterStats.HitboxRadius;

	private CharacterStats PlayerStats => GameWorld.Instance.MainPlayer.Character.CharacterStats;

	private float _invisibilityTime;

	public override void _Process(double delta)
	{
		_invisibilityTime -= (float)delta;
		ProcessContacts();
	}

	private void ProcessContacts()
	{
		if (_invisibilityTime > 0)
			return;

		if (!TargetQuery.TryGetTargetsInArea(Player.GlobalPosition, PlayerHitboxRadius, out var entities))
			return;

		var largestDamage = 0;
		var largestDamageId = Entity.Null;

		foreach (var entity in entities)
		{
			if (!GameWorld.World.TryGet<EnemyContactDamageComponent>(entity, out var damage))
				continue;
			if (damage.Damage > largestDamage)
			{
				largestDamage = damage.Damage;
				largestDamageId = entity;
			}
		}

		if (largestDamageId == Entity.Null)
			return;

		_invisibilityTime = PlayerStats.InvincibilityTime;
		PlayerStats.Damage(largestDamage);

		DamageFeedback();
	}

	private void DamageFeedback()
	{
		var sprite = Player.GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (sprite is null)
			return;

		if (sprite.Material is not ShaderMaterial spriteShaderMaterial)
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
