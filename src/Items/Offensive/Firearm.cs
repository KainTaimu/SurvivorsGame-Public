using Game.Core.Settings;
using Game.Levels.Controllers;
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

	public int MagazineCapacity => FirearmStats?.MagazineCapacity ?? 30;

	public int MagazineCount => _magazineCount;

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

	public bool IsReadyToShoot
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

	public FirearmStats? FirearmStats => Stats as FirearmStats;

	public string? AttackActionString { get; set; }

	protected double _fireCooldown;

	private int _magazineCount;

	private float ReloadTimeMs => FirearmStats?.ReloadTimeMs ?? 1500;
	private float BloomCoefficientDeg =>
		FirearmStats?.BloomCoefficientDeg ?? 0.03f;
	private float HorizontalRecoilMin =>
		FirearmStats?.HorizontalRecoilMin ?? 1f;
	private float HorizontalBaseRecoil =>
		FirearmStats?.HorizontalBaseRecoil ?? 3f;
	private float HorizontalRecoilRandom =>
		FirearmStats?.HorizontalRecoilRandom ?? 1f;
	private float VerticalRecoilMin => FirearmStats?.VerticalRecoilMin ?? 2f;
	private float VerticalBaseRecoil => FirearmStats?.VerticalBaseRecoil ?? 3f;
	private float VerticalRecoilRandom =>
		FirearmStats?.VerticalRecoilRandom ?? 0.1f;
	private float RecoilScale => FirearmStats?.RecoilScale ?? 1f;
	private float RecoilAccumilationScale =>
		FirearmStats?.RecoilAccumilationScale ?? 1f;
	private float CameraRecoilScale => FirearmStats?.CameraRecoilScale ?? 1f;

	protected Crosshair? Crosshair => Crosshair.Instance;

	protected ProjectilePool ProjectilePool = null!;

	public override void _Ready()
	{
		UpdateAdditionalFields();
		OnStatsChanged += UpdateAdditionalFields;
		_magazineCount = FirearmStats?.MagazineCapacity ?? 30;

		// HACK: Too lazy to add ProjectilePool for all existing Firearms.
		// Should avoid creating nodes programatically unless for pooling
		ProjectilePool = new() { ProjectileScene = _projectileScene };
		AddChild(ProjectilePool);
	}

	public override void Attack()
	{
		if (_magazineCount <= 0)
		{
			Reload();
			return;
		}
		if (!IsReadyToShoot)
			return;

		ShootAudioPlayer?.Play();

		_fireCooldown = Stats.AttackSpeed;
		_magazineCount--;

		var playerVector = Player.GetCanvasTransform() * Player.Position;

		Vector2 mouseVector;
		if (Crosshair is not null)
		{
			mouseVector =
				Crosshair.PrimaryCrosshairSprite.GetCanvasTransform()
				* Crosshair.PrimaryCrosshairSprite.GlobalPosition;
		}
		else
		{
			mouseVector = Player.GetGlobalMousePosition();
		}
		var rotation = playerVector.AngleToPoint(mouseVector);

		var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		rotation += bloom;

		var projectile = ProjectilePool.GetProjectile();

		projectile.Origin = this;
		projectile.SetScale(Vector2.One * Stats.ProjectileScaleMultiplier);
		projectile.SetPosition(Player.Position);
		projectile.SetRotation(rotation);
		projectile.ProjectileSpeed = Stats.ProjectileSpeed;
		projectile.PierceLimit = Stats.PierceLimit;
		projectile.HitRadius = FirearmStats?.ProjectileRadius ?? 24;

		ApplyCursorRecoil();
		EmitSignalOnAttack();
	}

	public void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount == MagazineCapacity)
			return;
		GetTree().CreateTimer(ReloadTimeMs / 1000f).Timeout += () =>
		{
			_magazineCount = MagazineCapacity;
			IsReloading = false;
		};
		IsReloading = true;
		ReloadAudioPlayer?.Play();
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
				HorizontalBaseRecoil
					+ GD.RandRange(
						-HorizontalRecoilRandom,
						HorizontalRecoilRandom
					)
			);
		recoilX = Math.Clamp(
			recoilX,
			-Math.Abs(HorizontalRecoilMin),
			float.MaxValue
		);

		var recoilY =
			VerticalBaseRecoil
			+ Math.Abs((float)GD.Randfn(0, VerticalRecoilRandom));
		recoilY = Math.Clamp(recoilY, VerticalRecoilMin, float.MaxValue);

		var recoil = new Vector2(recoilX, -recoilY) * RecoilScale;
		Crosshair.Recoil.ApplyImpulse(recoil);
	}

	// TODO: Move camera recoil to a method in PlayerCameraController
	// and have this call that instead.
	public void ApplyCameraRecoil()
	{
		if (!GameSettings.Instance.EnableCameraShake)
			return;
		if (CameraRecoilScale == 0)
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
				* CameraRecoilScale;

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
		_fireCooldown = 0;

		Logger.LogDebug(
			"Updated Stats\n",
			ClassInspector.GetClassPropertiesString(Stats)
		);
	}
}
