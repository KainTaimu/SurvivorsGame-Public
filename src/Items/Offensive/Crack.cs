using Arch.Core;
using Game.Players.Controllers;

namespace Game.Items.Offensive;

public partial class Crack : BaseOffensive, IManualAttack
{
	[Export]
	private StatusEffect _crackEffect = null!;

	[Export]
	private StatusEffect _permanentCrackEffect = null!;

	private bool _isPermanentCrackApplied;

	public string? AttackActionString { get; set; }

	public override void _Ready()
	{
		OnEquipped += () =>
		{
			if (_isPermanentCrackApplied)
				return;

			Player.StatusEffectController.AddStatusEffect(_permanentCrackEffect);
			_isPermanentCrackApplied = true;
		};
		OnUnequipped += () =>
		{
			if (!_isPermanentCrackApplied)
				return;

			Player.StatusEffectController.RemoveStatusEffect(_permanentCrackEffect);
			_isPermanentCrackApplied = false;
		};
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		if (!Input.IsActionPressed(AttackActionString))
			return;

		Player.StatusEffectController.RemoveStatusEffect(_crackEffect);
		Player.StatusEffectController.AddStatusEffect(_crackEffect);
	}

	protected override void HandleHitECS(Entity entity) { }
}
