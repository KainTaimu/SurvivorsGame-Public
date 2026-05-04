using Godot.Collections;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class BaseOffensiveStats : BaseItemStats
{
	[Export]
	public int Damage = 4;

	/// <summary>
	/// Damage is calculated as: Base Damage + (Base Damage * CritDamageMultiplier)
	/// </summary>
	[Export(PropertyHint.Range, "0,3,0.1,or_greater")]
	public float CritDamageMultiplier = 1.5f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float CritChanceProportion = 0.1f;

	[Export]
	public int ProjectileSpeed = 3600;

	[Export]
	public float ProjectileScaleMultiplier = 1;

	[Export]
	public int ProjectileRadius = 24;

	[Export]
	public float AttackSpeed = 0.2f;

	[Export]
	public int PierceLimit = 1;

	[Export]
	public Array<Resource> ProjectileEffects
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = [];

	[Export]
	public Dictionary<string, Variant> Additional
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = [];
}
