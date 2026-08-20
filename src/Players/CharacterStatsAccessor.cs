using Game.Levels.Controllers;

namespace Game.Players;

/// <summary>
/// A bridge that exposes CharacterStats properties for GDScript scripts to use
/// </summary>
[GlobalClass]
public partial class CharacterStatsAccessor : RefCounted
{
	private static CharacterStats Stats => GameWorld.Instance.MainPlayer.Character.CharacterStats;

	public int Health => Stats.Health;

	public int MaxHealth => Stats.MaxHealth;

	public float MoveSpeed => Stats.MoveSpeed;

	public float RunSpeed => Stats.RunSpeed;

	public float Stamina => Stats.Stamina;

	public float MaxStamina => Stats.MaxStamina;

	public int Defense => Stats.Defense;

	public float PickupRangeRadius => Stats.PickupRangeRadius;

	public int HealthRegenPerSecond => Stats.HealthRegenPerSecond;

	public float InvincibilityTime => Stats.InvincibilityTime;

	public float HitboxRadius => Stats.HitboxRadius;

	public float CriticalChance => Stats.CriticalChance;

	public float CriticalDamage => Stats.CriticalDamage;

	public float AttackSpeed => Stats.AttackSpeed;

	public float BloomScale => Stats.BloomScale;

	public float RecoilScale => Stats.RecoilScale;

	public float IncomingDamage => Stats.IncomingDamage;

	public float OutgoingDamage => Stats.OutgoingDamage;
}
