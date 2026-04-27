using Game.Items.Projectiles;
using Game.Players;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Ak47 : BaseOffensive, IReloadable
{
	[Export]
	private PackedScene _projectileScene = null!;

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
	private float _bloomCoefficient = 0.03f;
	private int _magazineCapacity = 30;
	private int _magazineCount;

	private float _horizontalBaseRecoil = 3f;
	private float _horizontalRecoilRandom = 1f;
	private float _verticalBaseRecoil = 3f;
	private float _verticalRecoilRandom = 0.1f;

	private Crosshair? Crosshair => Crosshair.Instance;

	public override void _Ready()
	{
		_fireCooldown = Stats.AttackSpeed;

		_magazineCapacity = Stats.Additional["MagazineCapacity"].As<int>();
		_magazineCount = _magazineCapacity;
		_reloadTimeMs = Stats.Additional["ReloadTimeMs"].As<int>();
		_bloomCoefficient = Stats.Additional["BloomCoefficient"].As<float>();

		_horizontalBaseRecoil = Stats
			.Additional["HorizontalBaseRecoil"]
			.As<float>();
		_horizontalRecoilRandom = Stats
			.Additional["HorizontalRecoilRandom"]
			.As<float>();
		_verticalBaseRecoil = Stats
			.Additional["VerticalBaseRecoil"]
			.As<float>();
		_verticalRecoilRandom = Stats
			.Additional["VerticalRecoilRandom"]
			.As<float>();
	}

	public override void _Process(double delta)
	{
		_fireCooldown -= delta;
		if (!Input.IsActionPressed(InputMapNames.PrimaryAttack))
			return;

		Attack();
	}

	protected override void Attack()
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

		var bloom = (float)GD.Randfn(rotation, _bloomCoefficient);

		var projectile = _projectileScene.Instantiate<ProjectileBullet>();
		projectile.Origin = this;
		projectile.SetScale(Vector2.One * Stats.ProjectileScaleMultiplier);
		projectile.SetPosition(Player.Position);
		projectile.SetRotation(bloom);
		projectile.ProjectileSpeed = Stats.ProjectileSpeed;
		projectile.PierceLimit = Stats.PierceLimit;
		AddChild(projectile);

		ApplyCursorRecoil();
	}

	public void Reload()
	{
		GetTree().CreateTimer(_reloadTimeMs / 1000).Timeout += () =>
			_magazineCount = _magazineCapacity;
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
		recoilY = Math.Clamp(recoilY, 2, float.MaxValue);

		var recoil = new Vector2(recoilX, -recoilY);
		Crosshair.Recoil.ApplyImpulse(recoil, 1f);
	}
}
