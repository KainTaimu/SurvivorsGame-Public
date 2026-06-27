using Game.Levels.Controllers;
using Game.Players;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class BaseOffensiveStats : BaseItemStats
{
	[Export]
	public int BaseDamage
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 4;

	[Export]
	public float BaseDamageVarianceMultiplier
	{
		get;
		set
		{

			field = value;
		}
	} = 0.15f;

	/// <summary>
	/// Damage is calculated as: Base Damage + (Base Damage * CritDamageMultiplier)
	/// </summary>
	[Export(PropertyHint.Range, "0,3,0.1,or_greater")]
	public float BaseCritDamageMultiplier
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1.5f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float BaseCritChanceProportion
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 0.1f;

	[Export]
	public int BaseProjectileSpeed
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 3600;

	[Export]
	public float BaseProjectileScaleMultiplier
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1;

	[Export]
	public int BaseProjectileRadius
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 24;

	[Export]
	public float BaseAttackSpeed
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 0.2f;

	[Export]
	public int BasePierceLimit
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1;

	private CharacterStats PlayerStats => GameWorld.Instance.MainPlayer.Character.CharacterStats;

	public int Damage => BaseDamage;
	public float DamageVarianceMultiplier => BaseDamageVarianceMultiplier;

	public float CritDamageMultiplier => BaseCritDamageMultiplier * PlayerStats.CriticalDamageMultiplier;
	public float CritChanceProportion => BaseCritChanceProportion * PlayerStats.CriticalChanceMultiplier;
	public int ProjectileSpeed => BaseProjectileSpeed;
	public float ProjectileScaleMultiplier => BaseProjectileScaleMultiplier;
	public int ProjectileRadius => BaseProjectileRadius;
	public float AttackSpeed => BaseAttackSpeed * PlayerStats.AttackSpeedMultiplier;
	public int PierceLimit => BasePierceLimit;
}
