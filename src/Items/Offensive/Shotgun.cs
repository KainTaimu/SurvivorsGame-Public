// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.UI;

namespace Game.Items.Offensive;

public partial class Shotgun : SimpleFirearm, IHasPumpActionStats
{
	[Export]
	public PumpActionStats PumpActionStats { get; set; } = null!;

	[Export]
	private AudioStreamPlayer? _shellReloadAudioPlayer;

	[Export]
	private AudioStreamPlayer? _cockingAudioPlayer;

	private double _reloadCooldown;

	private int PelletCount => PumpActionStats.PelletCount;

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
			GetTree().CreateTimer(AttackStats.AttackSpeed / 2).Timeout += () => _cockingAudioPlayer.Play();

		var playerPosition = Player.GetCanvasTransform() * Player.Position;

		var mouseVector = Crosshair?.CanvasSpacePosition ?? Player.GetGlobalMousePosition();

		var baseRotation = playerPosition.AngleToPoint(mouseVector);
		for (var i = 0; i < PelletCount; i++)
		{
			var bloomRad = BloomStats.BloomCoefficientDeg * (Math.PI / 180);
			var bloom = 0.0 + bloomRad / 2 * _rng.Randfn();
			var rotation = baseRotation + bloom;
			var scale = Vector2.One * ProjectileStats.Scale;
			var speed = ProjectileStats.Speed * (float)GD.RandRange(1f, 2f);

			_projectileAttack.Attack(
				Pool.GetProjectile,
				this,
				Player.GlobalPosition,
				(float)rotation,
				ProjectileStats.HitRadius,
				speed,
				PierceStats.PierceLimit,
				scale
			);
		}

		EmitSignalOnAttack();
		return true;
	}
}
