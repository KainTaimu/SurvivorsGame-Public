using System.Collections.Generic;
using Game.UI;

namespace Game.Items.Offensive;

public partial class Shotgun : SimpleFirearm
{
	[Export]
	private AudioStreamPlayer? _shellReloadAudioPlayer;

	[Export]
	private AudioStreamPlayer? _cockingAudioPlayer;

	private double _reloadCooldown;

	private int PelletCount => OffensiveStats.Additional.GetValueOrDefault("PelletCount").AsInt32();

	private static readonly RandomNumberGenerator _rng = new();

	private static Crosshair? Crosshair => Crosshair.Instance;

	public override bool Attack()
	{
		if (MagazineCount <= 0)
			return false;

		if (IsReloading)
			return false;

		if (MagazineCount <= 6)
			EmitSignalAlmostEmpty();

		MagazineCount--;
		if (MagazineCount <= 0)
			Reload();

		if (_cockingAudioPlayer is not null)
			GetTree().CreateTimer(OffensiveStats.AttackSpeed / 2).Timeout += () => _cockingAudioPlayer.Play();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();

		var baseRotation = playerPosition.AngleToPoint(mouseVector);
		for (var i = 0; i < PelletCount; i++)
		{
			var bloomRad = BloomCoefficientDeg * (Math.PI / 180);
			var bloom = 0.0 + (bloomRad / 2) * _rng.Randfn();
			var rotation = baseRotation + bloom;
			var scale = Vector2.One * FirearmStats.ProjectileScaleMultiplier;
			var speed = OffensiveStats.ProjectileSpeed * (float)GD.RandRange(1f, 2f);

			_projectileAttack.Attack(
				Pool.GetProjectile,
				Player.GlobalPosition,
				(float)rotation,
				FirearmStats.ProjectileRadius,
				speed,
				FirearmStats.PierceLimit,
				scale
			);
		}

		EmitSignalOnAttack();
		return true;
	}
}
