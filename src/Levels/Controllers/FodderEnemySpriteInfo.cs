namespace Game.Levels.Controllers;

[GlobalClass]
public partial class FodderEnemySpriteInfo : Resource
{
	[Export]
	public Texture2D SpriteFrames = null!;

	[Export]
	public string SpriteName = null!;

	[Export]
	public float AnimationSpeed = 0.5f;

	[Export]
	public byte FrameCountX = 1;

	[Export]
	public byte FrameSizePxX = 40;

	[Export]
	public byte FrameSizePxY = 40;

	[Export]
	public byte Opacity = 255;

	[Export]
	public byte Flash;
}
