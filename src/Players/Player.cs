using Game.Players.Controllers;

namespace Game.Players;

[GlobalClass]
public partial class Player : Node2D
{
	[Signal]
	public delegate void OnDamagedEventHandler(int damage);

	[Export]
	public Character Character { get; private set; } = null!;

	[Export]
	public PlayerMovementController MovementController { get; private set; } = null!;

	[Export]
	public AbstractPlayerWeaponController WeaponController { get; private set; } = null!;

	[Export]
	public PlayerStatusEffectController StatusEffectController { get; private set; } = null!;

	public bool IsAlive => Character.CharacterStats.Health == 0;
}
