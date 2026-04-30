using Game.Core.ECS;
using Game.Items.Projectiles;
using Game.Players;
using Game.UI;
using Game.Utils;

namespace Game.Items.Offensive;

public abstract partial class Firearm
	: BaseOffensive,
		IReloadable,
		IManualAttack
{
	[Signal]
	public delegate void OnReloadStartEventHandler();

	[Signal]
	public delegate void OnReloadEndEventHandler();

	[Export]
	private PackedScene _projectileScene = null!;

	[Export]
	public AudioStreamPlayer? ShootAudioPlayer;

	[Export]
	public AudioStreamPlayer? ReloadAudioPlayer;

	public int MagazineCapacity
	{
		get => _magazineCapacity;
	}

	public int MagazineCount
	{
		get => _magazineCount;
	}

	public bool IsReloading
	{
		get;
		set
		{
			if (value)
				EmitSignalOnReloadStart();
			else
				EmitSignalOnReloadEnd();
			field = value;
		}
	}

	public bool ReadyToShoot
	{
		get
		{
			if (_fireCooldown > 0)
				return false;
			if (IsReloading)
				return false;
			if (_magazineCount <= 0)
				return false;
			return true;
		}
	}

	public string? AttackActionString { get; set; }

	protected double _fireCooldown;

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
	private float _recoilAccumilationScale = 1f;
	private float _cameraRecoilScale = 1f;

	protected Crosshair? Crosshair => Crosshair.Instance;

	public override void _Ready()
	{
		UpdateAdditionalFields();
		OnStatsChanged += UpdateAdditionalFields;
	}

	public override void Attack()
	{
		if (_magazineCount <= 0)
		{
			Reload();
			return;
		}
		if (!ReadyToShoot)
			return;

		ShootAudioPlayer?.Play();

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
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount == MagazineCapacity)
			return;
		GetTree().CreateTimer(_reloadTimeMs / 1000f).Timeout += () =>
		{
			_magazineCount = _magazineCapacity;
			IsReloading = false;
		};
		IsReloading = true;
		ReloadAudioPlayer?.Play();
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
	}

	// BUG:
	// Extreme recoil due to accumilated impulse in Crosshair recoil system
	// if shooting two high recoil weapons at once
	private void ApplyCursorRecoil()
	{
		if (Crosshair is null)
			return;

		var recoilX = (float)
			GD.Randfn(
				0,
				_horizontalBaseRecoil
					+ GD.RandRange(
						-_horizontalRecoilRandom,
						_horizontalRecoilRandom
					)
			);
		recoilX = Math.Clamp(
			recoilX,
			-Math.Abs(_horizontalRecoilMin),
			float.MaxValue
		);

		var recoilY =
			_verticalBaseRecoil
			+ Math.Abs((float)GD.Randfn(0, _verticalRecoilRandom));
		recoilY = Math.Clamp(recoilY, _verticalRecoilMin, float.MaxValue);

		var recoil = new Vector2(recoilX, -recoilY) * _recoilScale;
		Crosshair.Recoil.ApplyImpulse(recoil);
	}

	public void ApplyCameraRecoil()
	{
		if (_cameraRecoilScale == 0)
			return;

		var camera = GetViewport().GetCamera2D();

		var origPos = camera.Position;
		var tween = CreateTween().SetTrans(Tween.TransitionType.Spring);

		for (var i = 0; i < 6; i++)
		{
			static int rand() => GD.RandRange(-1, 1);
			var shake =
				new Vector2(rand(), rand())
				* GD.RandRange(4, 9)
				* _cameraRecoilScale;

			tween.TweenProperty(
				camera,
				"offset",
				camera.Position + shake,
				1 / 30f
			);
		}
		tween.TweenProperty(camera, "offset", origPos, 1 / 8f);
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

		if (Stats.Additional.TryGetValue("RecoilAccumilationScale", out var x))
			_recoilAccumilationScale = x.AsSingle();
		if (Stats.Additional.TryGetValue("CameraRecoilScale", out x))
			_cameraRecoilScale = x.AsSingle();

		Logger.LogDebug(
			"Updated Stats",
			GetClassProperties.GetClassPropertiesString(Stats)
		);
	}
}
