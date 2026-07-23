using Arch.Core;
using Game.Core.ECS;
using Game.Levels.Controllers;
using Game.UI;

namespace Game.Items.Offensive;

public partial class GrenadeLauncher : BaseOffensive, IManualAttack, IReloadable
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Signal]
	public delegate void AlmostEmptyEventHandler();

	[Export]
	private float _throwForce = 1250;

	[Export]
	public PackedScene GrenadeScene = null!;

	[Export]
	private AbstractFireGroup _fireGroup = null!;

	[Export]
	private AudioStreamPlayer? _explosionPlayer;

	public FirearmStats FirearmStats => (FirearmStats)OffensiveStats;
	private Crosshair? Crosshair => Crosshair.Instance;

	public string? AttackActionString { get; set; }

	public bool IsReloading { get; private set; }

	public int MagazineCapacity => FirearmStats.MagazineCapacity;

	public int MagazineCount
	{
		get
		{
			// YUCK
			if (field == int.MinValue)
				field = MagazineCapacity;
			return field;
		}
		set => field = field == int.MinValue ? MagazineCapacity : value;
	} = int.MinValue;

	public override void _Ready()
	{
		if (_fireGroup is ICooldown c)
			c.CooldownDuration = FirearmStats.AttackSpeed;

		FirearmStats.Changed += () =>
		{
			if (_fireGroup is ICooldown cooldown)
				cooldown.CooldownDuration = FirearmStats.AttackSpeed;
		};

		OnAttack += () => OffensiveEffects.ApplyCameraShake(FirearmStats.CameraRecoilScale, GetViewport, CreateTween);
		OnAttack += () =>
		{
			if (Crosshair is not null)
			{
				OffensiveEffects.ApplyCrosshairRecoil(
					Crosshair,
					FirearmStats.HorizontalBaseRecoil,
					FirearmStats.HorizontalRecoilMin,
					FirearmStats.HorizontalRecoilRandom,
					FirearmStats.VerticalBaseRecoil,
					FirearmStats.VerticalRecoilMin,
					FirearmStats.VerticalRecoilRandom,
					FirearmStats.RecoilScale,
					FirearmStats.RecoilAccumilationScale,
					FirearmStats.HorizontalRecoilPunish
				);
			}
		};
	}

	public override void _Process(double delta)
	{
		if (AttackActionString is null)
			return;

		if (_fireGroup is ICooldown fireGroupCooldown)
			fireGroupCooldown.Process((float)delta);

		if (Input.IsActionPressed(InputMapNames.WeaponReload))
		{
			Reload();
			return;
		}

		if (IsReloading)
			return;

		if (_fireGroup is IFireQueuable { CanFireQueued: true })
		{
			if (!_fireGroup.TryFire())
				return;
			Attack();
			return;
		}

		if (!Input.IsActionPressed(AttackActionString))
			return;

		if (_fireGroup.TryFire())
			Attack();
	}

	private void Attack()
	{
		if (Crosshair is null)
		{
			Logger.LogError("No crosshair");
			return;
		}

		MagazineCount--;
		if (MagazineCount == 0)
			Reload();

		var nade = GrenadeScene.Instantiate<Grenade>();
		nade.OnExploded += (_) =>
		{
			_explosionPlayer?.Reparent(GetTree().Root);
			_explosionPlayer?.Play();
		};

		nade.OffensiveOrigin = this;
		nade.GlobalPosition = Player.GlobalPosition;
		var mouseVector = Crosshair?.GlobalSpacePosition ?? Player.GetGlobalMousePosition();

		var bloomRad = FirearmStats.BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		var force = Vector2.Right.Rotated(nade.GlobalPosition.AngleToPoint(mouseVector) + bloom) * _throwForce;
		nade.LinearVelocity = force;
		GetTree().Root.Call(Window.MethodName.AddChild, nade);
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		GetTree().CreateTimer(FirearmStats.ReloadTime, false).Timeout += () =>
		{
			MagazineCount = MagazineCapacity;
			IsReloading = false;
			EmitSignalOnReloadEnd();
		};
		IsReloading = true;
	}

	protected override void HandleHitECS(Entity entity)
	{
		OffensiveEffects.ApplyDamage(
			entity,
			OffensiveStats.Damage,
			CalculateCrit(),
			OffensiveStats.DamageVarianceMultiplier,
			PlayerStats.OutgoingDamageMultiplier
		);

		OffensiveEffects.ApplyVelocityMultiplier(entity, 0f);

		if (!GameWorld.World.TryGet<HealthComponent>(entity, out var health))
			return;
		if (health.Health > 0)
			return;
		GameWorld.World.Add(entity, new DeathCauseComponent(DeathCauseEnum.Explosion));
	}
}
