using Game.Core.Settings;
using Game.UI;

namespace Game.Items.Offensive;

public abstract partial class AbstractFirearm : BaseOffensive, IManualAttack
{
	public int MagazineCapacity => FirearmStats.MagazineCapacity;

	public int MagazineCount
	{
		get
		{
			// YUCK
			if (field == int.MinValue)
				field = MagazineCapacity;
			return field;
		}
		set
		{
			// HACK:
			// MagazineCount would've been set in AbstractFirearm._Ready,
			// but I don't want to override _Ready because other inheriting AbstractFirearm classes might
			// override it themselves and I might forget to base._Ready()
			if (field == int.MinValue)
				field = MagazineCapacity;
			else
				field = value;
		}
	} = int.MinValue;

	public FirearmStats FirearmStats => (FirearmStats)OffensiveStats;

	public string? AttackActionString { get; set; }

	protected float BloomCoefficientDeg => FirearmStats.BloomCoefficientDeg * PlayerStats.BloomMultiplier;

	protected float HorizontalRecoilMin => FirearmStats.HorizontalRecoilMin;

	protected float HorizontalBaseRecoil => FirearmStats.HorizontalBaseRecoil;

	protected float HorizontalRecoilRandom => FirearmStats.HorizontalRecoilRandom;

	protected float VerticalRecoilMin => FirearmStats.VerticalRecoilMin;

	protected float VerticalBaseRecoil => FirearmStats.VerticalBaseRecoil;

	protected float VerticalRecoilRandom => FirearmStats.VerticalRecoilRandom;

	protected float RecoilScale => FirearmStats.RecoilScale * PlayerStats.RecoilMultiplier;

	protected float RecoilAccumilationScale => FirearmStats.RecoilAccumilationScale;

	protected bool HorizontalRecoilPunish => FirearmStats.HorizontalRecoilPunish;

	protected float CameraRecoilScale =>
		FirearmStats.CameraRecoilScale * PlayerStats.RecoilMultiplier * GameSettings.Instance.CameraShakeScale;

	protected bool ChamberLoaded => FirearmStats.ChamberLoaded;
}
