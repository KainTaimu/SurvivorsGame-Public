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

	public float MoveSpeedMultiplier => Stats.MoveSpeedMultiplier;

	public float RunSpeedMultiplier => Stats.RunSpeedMultiplier;

	public float StaminaMultiplier => Stats.StaminaMultiplier;

	public float MaxStaminaMultiplier => Stats.MaxStaminaMultiplier;

	public float IncomingDamageMultiplier => Stats.IncomingDamageMultiplier;

	public float OutgoingDamageMultiplier => Stats.OutgoingDamageMultiplier;

	public float CriticalChanceMultiplier => Stats.CriticalChanceMultiplier;

	public float CriticalDamageMultiplier => Stats.CriticalDamageMultiplier;

	public float AttackSpeedMultiplier => Stats.AttackSpeedMultiplier;

	public float BloomMultiplier => Stats.BloomMultiplier;

	public float RecoilMultiplier => Stats.RecoilMultiplier;

	public float XpMultiplier => Stats.XpMultiplier;
}
