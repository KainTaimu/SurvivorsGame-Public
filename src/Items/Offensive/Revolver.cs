using System.Collections.Generic;
using Game.Core.ECS;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Revolver : Firearm
{
	[Export]
	private RevolverAmmoCount _ammoCount = null!;

	[Export]
	public AudioStreamPlayer? CockAudioPlayer;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += () =>
		{
			GetTree().CreateTimer(_fireCooldown).Timeout += () =>
			{
				_ammoCount.RotateCylinder();
				if (MagazineCount != 0)
					CockAudioPlayer?.Play();
			};
			ApplyCameraRecoil();
		};

		// Reload SFX
		OnReloadStart += () =>
		{
			for (var i = 0; i < 6; i++)
				GetTree()
					.CreateTimer(
						FirearmStats?.ReloadTimeMs
							?? 0
								/ 1000 // To seconds
								/ 5 // Arbitrary
								/ 6 // Amount of cylinder spin
								* i
					)
					.Timeout += () => CockAudioPlayer?.Play();
		};
		OnReloadEnd += () =>
		{
			for (var i = 0; i < 6; i++)
				GetTree()
					.CreateTimer(
						FirearmStats?.ReloadTimeMs ?? 0 / 1000 / 5 / 6 * i
					)
					.Timeout += () => CockAudioPlayer?.Play();
		};
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}
		if (
			!Input.IsActionPressed(
				AttackActionString ?? InputMapNames.PrimaryAttack
			)
		)
			return;
		Attack();
	}

	protected override void HandleHitECS(int id)
	{
		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			return;
		var knockback = Stats
			.Additional.GetValueOrDefault("Knockback")
			.AsSingle();
		var knockbackVector = Player.GlobalPosition.DirectionTo(pos.Position);
		knockbackVector *= knockback;

		ComponentStore.SetComponent(
			id,
			pos with
			{
				Position = pos.Position + knockbackVector,
			}
		);
	}
}
