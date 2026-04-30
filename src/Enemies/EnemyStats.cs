namespace Game.Enemies;

[GlobalClass]
public partial class EnemyStats : Resource
{
	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int DamageOnContact = 2;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int MaxHealth = 5;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float MoveSpeed = 180;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int Defense;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int XpDrop = 10;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public int HealthRegenPerSecond;

	[ExportCategory("Multiplier attributes")]
	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float HealthMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float MoveSpeedMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float IncomingDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float OutgoingDamageMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float AttackSpeedMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float ProjectileMultiplier = 1;

	[Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
	public float XpDropMultiplier = 1;
}
