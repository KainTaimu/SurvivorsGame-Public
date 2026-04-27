using Godot.Collections;

namespace Game.Items.Offensive;

[GlobalClass]
public partial class BaseOffensiveStats : BaseItemStats
{
	[Export]
	public int Damage
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export(PropertyHint.Range, "0,3,0.1,or_greater")]
	public float CritDamageMultiplier
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float CritChanceProportion
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export]
	public int ProjectileSpeed
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export]
	public float ProjectileScaleMultiplier
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export]
	public float AttackSpeed
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export]
	public int PierceLimit
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1;

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
