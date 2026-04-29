using System.Collections.Generic;
using Game.Core.ECS;
using Game.Items.Projectiles;
using Game.Levels.Controllers;
using Game.Players;
using Game.SFX;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Ak47 : BaseOffensive, IReloadable
{
	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	private RandomAudioStreamPlayer _streamPlayer = null!;

	public int MagazineCapacity
	{
		get => _magazineCapacity;
	}

	public int MagazineCount
	{
		get => _magazineCount;
	}

	private double _fireCooldown;
	private bool _isReloading;

	private int _reloadTimeMs = 1500;
	private float _bloomCoefficientDeg = 0.03f;
	private int _magazineCapacity = 30;
	private int _magazineCount;

	private float _horizontalRecoilMin = 1f;
	private float _horizontalBaseRecoil = 3f;
	private float _horizontalRecoilRandom = 1f;
	private float _verticalRecoilMin = 2f;
	private float _verticalBaseRecoil = 3f;
	private float _verticalRecoilRandom = 0.1f;
	private float _recoilScale = 1f;

	private Crosshair? Crosshair => Crosshair.Instance;
	private EnemyTargetQuery TargetQuery => EnemyTargetQuery.Instance;
	private EntityComponentStore ComponentStore =>
		EntityComponentStore.Instance;

	public override void _Ready()
	{
		UpdateAdditionalFields();
		OnStatsChanged += UpdateAdditionalFields;
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;

		Attack();
	}

	public override void Attack()
	{
		if (_fireCooldown > 0)
			return;
		if (_isReloading)
			return;
		if (_magazineCount <= 0)
		{
			Reload();
			return;
		}

		_streamPlayer?.PlayRandom();
		_streamPlayer?.PitchScale = 0.9f + (GD.RandRange(-1, 1) * 0.1f);

		_fireCooldown = Stats.AttackSpeed;
		_magazineCount--;

		var playerVector = Player.GetCanvasTransform() * Player.Position;

		Vector2 mouseVector;
		if (Crosshair is not null)
		{
			mouseVector =
				Crosshair.CrosshairSprite.GetCanvasTransform()
				* Crosshair.CrosshairSprite.GlobalPosition;
		}
		else
		{
			mouseVector = Player.GetGlobalMousePosition();
		}
		var rotation = playerVector.AngleToPoint(mouseVector);

		var bloomRad = _bloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		rotation += bloom;

		var projectile = _projectileScene.Instantiate<ProjectileBullet>();
		projectile.Origin = this;
		projectile.SetScale(Vector2.One * Stats.ProjectileScaleMultiplier);
		projectile.SetPosition(Player.Position);
		projectile.SetRotation(rotation);
		projectile.ProjectileSpeed = Stats.ProjectileSpeed;
		projectile.PierceLimit = Stats.PierceLimit;
		AddChild(projectile);

		ApplyCursorRecoil();
	}

	public void Reload()
	{
		GetTree().CreateTimer(_reloadTimeMs / 1000).Timeout += () =>
		{
			_magazineCount = _magazineCapacity;
			_isReloading = false;
		};
		_isReloading = true;
	}

	protected override void HandleHitECS(int id)
	{
		if (!ComponentStore.GetComponent<HealthComponent>(id, out var health))
			return;

		var hit = new HitFeedbackComponent() { HitTime = 0.5f };
		if (!ComponentStore.GetComponent<HitFeedbackComponent>(id, out var _))
			ComponentStore.RegisterComponent(id, hit);
		else
			ComponentStore.SetComponent(id, hit);

		var crit = CalculateCrit();
		var newHealth = health.Health - Stats.Damage - crit;

		ComponentStore.SetComponent(id, health with { Health = newHealth });

		if (!ComponentStore.GetComponent<PositionComponent>(id, out var pos))
			return;
		var knockback = Stats
			.Additional.GetValueOrDefault("Knockback")
			.AsSingle();
		var knockbackVector =
			GameWorld.Instance.MainPlayer.GlobalPosition.DirectionTo(
				pos.Position
			);
		knockbackVector *= knockback;

		ComponentStore.SetComponent(
			id,
			pos with
			{
				Position = pos.Position + knockbackVector,
			}
		);
	}

	private void ApplyCursorRecoil()
	{
		if (Crosshair is null)
			return;

		var recoilX =
			_horizontalBaseRecoil
			* (float)GD.Randfn(0, _horizontalRecoilRandom);
		var recoilY =
			_verticalBaseRecoil * (float)GD.Randfn(1, _verticalRecoilRandom);
		recoilY = Math.Clamp(recoilY, _verticalRecoilMin, float.MaxValue);

		var recoil = new Vector2(recoilX, -recoilY) * _recoilScale;
		Crosshair.Recoil.ApplyImpulse(recoil);
	}

	private void UpdateAdditionalFields()
	{
		_fireCooldown = Stats.AttackSpeed;

		_magazineCapacity = Stats.Additional["MagazineCapacity"].As<int>();
		_magazineCount = _magazineCapacity;
		_reloadTimeMs = Stats.Additional["ReloadTimeMs"].As<int>();
		_bloomCoefficientDeg = Stats
			.Additional["BloomCoefficientDeg"]
			.As<float>();

		_horizontalRecoilMin = Stats
			.Additional["HorizontalRecoilMin"]
			.AsSingle();
		_horizontalBaseRecoil = Stats
			.Additional["HorizontalBaseRecoil"]
			.As<float>();
		_horizontalRecoilRandom = Stats
			.Additional["HorizontalRecoilRandom"]
			.As<float>();
		_verticalRecoilMin = Stats.Additional["VerticalRecoilMin"].AsSingle();
		_verticalBaseRecoil = Stats
			.Additional["VerticalBaseRecoil"]
			.As<float>();
		_verticalRecoilRandom = Stats
			.Additional["VerticalRecoilRandom"]
			.As<float>();
		_recoilScale = Stats.Additional["RecoilScale"].AsSingle();
	}
}
