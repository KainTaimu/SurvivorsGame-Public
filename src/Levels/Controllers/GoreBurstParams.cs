namespace Game.Levels.Controllers;

[GlobalClass]
public partial class GoreBurstParams : Resource
{
	[Export]
	public int Count = 120;

	[ExportSubgroup("Velocity")]
	[Export]
	public float SpeedMin;

	[Export]
	public float SpeedMax = 600f;

	[ExportSubgroup("Scale")]
	[Export]
	public float ScaleMin = 0.1f;

	[Export]
	public float ScaleMax = 4f;

	[ExportSubgroup("Emission")]
	[Export]
	public float EmitRadius = 1f;

	[Export]
	public Vector2 EmitOffset = new(0, -8);

	[ExportSubgroup("Settle")]
	[Export]
	public float SettleMin = 0.02f;

	[Export]
	public float SettleMax = 0.16f;

	[ExportSubgroup("Direction")]
	[Export]
	public float SpreadDegrees = 38.5f;

	[ExportSubgroup("Color")]
	[Export]
	public Color BaseTint = new(0.7819659f, 0f, 0f);

	// Random per-particle hue shift added to BaseTint's hue.
	[Export]
	public float HueVariationMin = -0.12f;

	[Export]
	public float HueVariationMax = -0.05f;
}
