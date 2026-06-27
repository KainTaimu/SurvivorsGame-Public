namespace Game.Items.Offensive;

[GlobalClass]
public partial class FirearmStats : BaseOffensiveStats
{
	[ExportGroup("AbstractFirearm")]
	[Export]
	public float ReloadTime
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1.5f;

	[Export]
	public float BloomCoefficientDeg
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	}

	[Export]
	public int MagazineCapacity
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 30;

	[Export]
	public float HorizontalRecoilMin
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1f;

	[Export]
	public float HorizontalBaseRecoil
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 3f;

	[Export]
	public float HorizontalRecoilRandom
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1f;

	[Export]
	public float VerticalRecoilMin
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 2f;

	[Export]
	public float VerticalBaseRecoil
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 3f;

	[Export]
	public float VerticalRecoilRandom
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 0.1f;

	[Export]
	public float RecoilScale
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1f;

	[Export]
	public float RecoilAccumilationScale
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1f;

	[Export]
	public bool HorizontalRecoilPunish
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = true;

	[Export]
	public float CameraRecoilScale
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = 1f;

	[Export]
	public bool ChamberLoaded
	{
		get;
		set
		{
			field = value;
			EmitChanged();
		}
	} = true;
}
