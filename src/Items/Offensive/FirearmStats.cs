namespace Game.Items.Offensive;

[GlobalClass]
public partial class FirearmStats : BaseOffensiveStats
{
	[ExportGroup("Firearm")]
	[Export]
	public int ProjectileRadius = 24;

	[Export]
	public int ReloadTimeMs = 1500;

	[Export]
	public float BloomCoefficientDeg = 0.03f;

	[Export]
	public int MagazineCapacity = 30;

	[Export]
	public float HorizontalRecoilMin = 1f;

	[Export]
	public float HorizontalBaseRecoil = 3f;

	[Export]
	public float HorizontalRecoilRandom = 1f;

	[Export]
	public float VerticalRecoilMin = 2f;

	[Export]
	public float VerticalBaseRecoil = 3f;

	[Export]
	public float VerticalRecoilRandom = 0.1f;

	[Export]
	public float RecoilScale = 1f;

	[Export]
	public float RecoilAccumilationScale = 1f;

	[Export]
	public float CameraRecoilScale = 1f;
}
