using System.Collections.Generic;
using Game.Core.ECS;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Revolver : Firearm
{
	[Export]
	private RevolverAmmoCount _ammoCount = null!;

	public override void _Ready()
	{
		base._Ready();
		OnAttack += () =>
		{
			_ammoCount.RotateCylinder();
			ShakeCamera();
		};
	}

	public override void _Process(double delta)
	{
		FireCooldown -= delta;
		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}
		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;

		Attack();
	}

	protected override void HandleHitECS(int id)
	{
		base.HandleHitECS(id);

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

	private void ShakeCamera()
	{
		var camera = GetViewport().GetCamera2D();

		var origPos = camera.Position;
		var tween = CreateTween().SetTrans(Tween.TransitionType.Spring);

		for (var i = 0; i < 6; i++)
		{
			static int rand() => GD.RandRange(-1, 1);
			var shake = new Vector2(rand(), rand()) * GD.RandRange(4, 9);

			tween.TweenProperty(
				camera,
				"position",
				camera.Position + shake,
				1 / 30f
			);
		}
		tween.TweenProperty(camera, "position", origPos, 1 / 8f);
	}
}
