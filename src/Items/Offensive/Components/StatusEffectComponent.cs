using Game.Levels.Controllers;
using Game.Players;
using Game.Players.Controllers;

namespace Game.Items.Offensive.Components;

[GlobalClass]
public partial class StatusEffectComponent : Node
{
	[Export]
	private StatusEffect _statusEffect = null!;
	private bool _isStatusEffectApplied;

	private static Player Player => GameWorld.Instance.MainPlayer;

	public void GiveStatusEffect()
	{
		if (_isStatusEffectApplied)
			return;

		Player.StatusEffectController.AddStatusEffect(_statusEffect);
		_isStatusEffectApplied = true;
	}

	public void RemoveStatusEffect()
	{
		if (!_isStatusEffectApplied)
			return;

		Player.StatusEffectController.RemoveStatusEffect(_statusEffect);
		_isStatusEffectApplied = false;
	}
}
