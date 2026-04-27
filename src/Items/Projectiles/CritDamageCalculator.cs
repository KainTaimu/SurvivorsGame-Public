namespace Game.Items.Projectiles;

public static class CritDamageCalculator
{
	public static int GetCritDamage(
		int baseDamage,
		float critDamageMultiplier,
		float critChance
	)
	{
		var roll = GD.Randf();
		if (roll > critChance)
			return 0;

		return (int)Mathf.Round(baseDamage * critDamageMultiplier);
	}
}
