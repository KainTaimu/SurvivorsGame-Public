using Game.Players.Controllers;
using Godot.Collections;

namespace Game.Levels.Controllers.Waves;

[GlobalClass]
public partial class StatusEffectWaveCompletionReward : AbstractWaveCompletionReward
{
	[Export]
	private Array<StatusEffect> _statusEffects = [];

	public override void GiveReward()
	{
		foreach (var statusEffect in _statusEffects)
		{
			GameWorld.Instance.MainPlayer.StatusEffectController.AddStatusEffect(statusEffect);
		}
	}
}
