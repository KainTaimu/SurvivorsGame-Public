using System.Diagnostics.CodeAnalysis;
using Game.Core.Settings;
using Game.Levels.Controllers;
using Game.UI;
using Game.Utils;

namespace Game.Items.Offensive;

public abstract partial class Firearm : BaseOffensive, IReloadable, IManualAttack
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

	[Export]
	public AudioStreamPlayer? AlmostEmptyAudioPlayer;

	[Export]
	public GpuParticles2D? SpentCasingParticles;

	public int MagazineCapacity => FirearmStats.MagazineCapacity;

	public int MagazineCount { get; set; }

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
			if (FireCooldown > 0)
				return false;
			if (IsReloading)
				return false;
			if (MagazineCount <= 0)
				return false;
			return true;
		}
	}

	public FirearmStats FirearmStats => (Stats as FirearmStats)!;

	public string? AttackActionString { get; set; }

	protected double FireCooldown;

	protected float ReloadTime => FirearmStats.ReloadTime;

	protected float BloomCoefficientDeg => FirearmStats.BloomCoefficientDeg;

	protected float HorizontalRecoilMin => FirearmStats.HorizontalRecoilMin;

	protected float HorizontalBaseRecoil => FirearmStats.HorizontalBaseRecoil;

	protected float HorizontalRecoilRandom => FirearmStats.HorizontalRecoilRandom;

	protected float VerticalRecoilMin => FirearmStats.VerticalRecoilMin;

	protected float VerticalBaseRecoil => FirearmStats.VerticalBaseRecoil;

	protected float VerticalRecoilRandom => FirearmStats.VerticalRecoilRandom;

	protected float RecoilScale => FirearmStats.RecoilScale;

	protected float RecoilAccumilationScale => FirearmStats.RecoilAccumilationScale;

	protected float CameraRecoilScale => FirearmStats.CameraRecoilScale * GameSettings.Instance.CameraShakeScale;

	protected Crosshair? Crosshair => Crosshair.Instance;

	protected ProjectilePool ProjectilePool = null!;

	public override void _Ready()
	{
		UpdateAdditionalFields();
		OnStatsChanged += UpdateAdditionalFields;
		MagazineCount = FirearmStats.MagazineCapacity;

		// HACK: Too lazy to add ProjectilePool for all existing Firearms.
		// Should avoid creating nodes programatically unless for pooling
		ProjectilePool = new ProjectilePool { ProjectileScene = _projectileScene };
		AddChild(ProjectilePool);
	}

	public override void Attack()
	{
		if (!IsReadyToShoot)
			return;

		ShootAudioPlayer?.Play();
		if (MagazineCount <= 6)
			AlmostEmptyAudioPlayer?.Play();

		FireCooldown = OffensiveStats.AttackSpeed;
		MagazineCount--;

		if (MagazineCount == 0)
			Reload();

		var playerVector = Player.GetCanvasTransform() * Player.Position;

		Vector2 mouseVector;
		if (Crosshair is not null)
		{
			mouseVector =
				Crosshair.PrimaryCrosshairSprite.GetCanvasTransform() * Crosshair.PrimaryCrosshairSprite.GlobalPosition;
		}
		else
			mouseVector = Player.GetGlobalMousePosition();

		var rotation = playerVector.AngleToPoint(mouseVector);

		var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
		var bloom = (float)GD.RandRange(-bloomRad / 2, bloomRad / 2);

		rotation += bloom;

		var projectile = ProjectilePool.GetProjectile();

		projectile.Origin = this;
		projectile.SetScale(Vector2.One * OffensiveStats.ProjectileScaleMultiplier);
		projectile.SetPosition(Player.Position);
		projectile.SetRotation(rotation);
		projectile.ProjectileSpeed = OffensiveStats.ProjectileSpeed;
		projectile.PierceLimit = OffensiveStats.PierceLimit;
		projectile.HitRadius = FirearmStats.ProjectileRadius;
		projectile.Initialize();

		ApplyCursorRecoil();
		SpawnCasingParticle();
		EmitSignalOnAttack();
	}

	[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
	public void SpawnCasingParticle()
	{
		if (SpentCasingParticles is null)
			return;
		var rotation = (Crosshair?.AngleFromPlayer ?? 0) + (Mathf.Pi / 2f);
		rotation += (float)GD.RandRange(-15f, 15f) * (Mathf.Pi / 180f);
		var transform = new Transform2D(rotation, Player.GlobalPosition).ScaledLocal(Vector2.One * 0.5f);
		var velocity = Vector2.Right * (float)GD.RandRange(300f, 1000f);
		velocity = velocity.Rotated(rotation);
		velocity += Player.MovementController.Velocity;
		Logger.LogDebug(
			$"Casing particle spawned at {Player.GlobalPosition} with rotation {rotation * (180 / Math.PI)}"
		);

		SpentCasingParticles.EmitParticle(
			transform,
			velocity,
			Colors.Black,
			Colors.Black,
			(uint)(
				GpuParticles2D.EmitFlags.RotationScale
				| GpuParticles2D.EmitFlags.Position
				| GpuParticles2D.EmitFlags.Velocity
			)
		);
	}

	public virtual void Reload()
	{
		if (IsReloading)
			return;
		if (MagazineCount >= MagazineCapacity)
			return;
		GetTree().CreateTimer(ReloadTime, false).Timeout += () =>
		{
			if (MagazineCount == 0)
				MagazineCount = MagazineCapacity;
			else
				MagazineCount = MagazineCapacity + 1; // Round in chamber
			IsReloading = false;
		};
		IsReloading = true;
		ReloadAudioPlayer?.Play();
	}

	// BUG:
	// Extreme recoil due to accumilated impulse in Crosshair recoil system
	// if shooting two high recoil weapons at once
	protected void ApplyCursorRecoil()
	{
		if (Crosshair is null)
			return;

		var recoilX = (float)
			GD.Randfn(0, HorizontalBaseRecoil + GD.RandRange(-HorizontalRecoilRandom, HorizontalRecoilRandom));
		recoilX = Math.Clamp(recoilX, -Math.Abs(HorizontalRecoilMin), float.MaxValue);

		var recoilY = VerticalBaseRecoil + Math.Abs((float)GD.Randfn(0, VerticalRecoilRandom));
		recoilY = Math.Clamp(recoilY, VerticalRecoilMin, float.MaxValue);

		var recoil = new Vector2(recoilX, -recoilY) * RecoilScale;
		Crosshair.Recoil.ApplyImpulse(recoil);
	}

	// TODO: Move camera recoil to a method in PlayerCameraController
	// and have this call that instead.
	protected void ApplyCameraRecoil()
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
			static int Rand()
			{
				return GD.RandRange(-1, 1);
			}

			var shake = new Vector2(Rand(), Rand()) * GD.RandRange(4, 9) * CameraRecoilScale;

			tween.TweenProperty(camera, "offset", camera.Position + shake, 1 / 30f);
		}

		tween.TweenProperty(camera, "offset", origPos, 1 / 8f);
	}

	private void UpdateAdditionalFields()
	{
		FireCooldown = 0;

		Logger.LogDebug("Updated Stats\n", ClassInspector.GetClassPropertiesString(Stats));
	}
}
