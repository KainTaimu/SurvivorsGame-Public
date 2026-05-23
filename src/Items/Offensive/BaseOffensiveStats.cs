using Game.Levels.Controllers;
using Game.Players;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class BaseOffensiveStats : BaseItemStats
{
	[Export]
	public int BaseDamage = 4;

	/// <summary>
	/// Damage is calculated as: Base Damage + (Base Damage * CritDamageMultiplier)
	/// </summary>
	[Export(PropertyHint.Range, "0,3,0.1,or_greater")]
	public float BaseCritDamageMultiplier = 1.5f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float BaseCritChanceProportion = 0.1f;

	[Export]
	public int BaseProjectileSpeed = 3600;

	[Export]
	public float BaseProjectileScaleMultiplier = 1;

	[Export]
	public int BaseProjectileRadius = 24;

	[Export]
	public float BaseAttackSpeed = 0.2f;

	[Export]
	public int BasePierceLimit = 1;

	private CharacterStats PlayerStats => GameWorld.Instance.MainPlayer.Character.CharacterStats;

	public int Damage => BaseDamage;
	public float CritDamageMultiplier => BaseCritDamageMultiplier * PlayerStats.CriticalDamageMultiplier;
	public float CritChanceProportion => BaseCritChanceProportion * PlayerStats.CriticalChanceMultiplier;
	public int ProjectileSpeed => BaseProjectileSpeed;
	public float ProjectileScaleMultiplier => BaseProjectileScaleMultiplier;
	public int ProjectileRadius => BaseProjectileRadius;
	public float AttackSpeed => BaseAttackSpeed * PlayerStats.AttackSpeedMultiplier;
	public int PierceLimit => BasePierceLimit;
}
