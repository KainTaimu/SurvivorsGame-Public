using PauseController = SurvivorsGame.UI.Menus.PauseController;

namespace SurvivorsGame.UI;

public partial class Crosshair : Node2D
{
    [Export]
    public AnimatedSprite2D CrosshairSprite { get; private set; }

    [Export(PropertyHint.Range, "0,5,0.25")]
    private float CrosshairSize = 4;

    public CrossHairRecoil Recoil { get; private set; }

    public Crosshair()
    {
        if (Instance != null)
        {
            Logger.LogError("Cannot have multiple instances of a singleton!");
            QueueFree();
            return;
        }

        Instance = this;
        Recoil = new CrossHairRecoil(this);
    }

    public static Crosshair Instance { get; private set; }

    public override void _Ready()
    {
        PauseController.Instance.Paused += HideCrosshair;
        PauseController.Instance.Unpaused += ShowCrosshair;

        if (!Engine.IsEditorHint())
        {
            Input.SetMouseMode(Input.MouseModeEnum.Hidden);
        }
    }

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();
        Position = mousePos;
    }

    public void ChangeCrosshairSize(float newSize)
    {
        CrosshairSprite.Scale = new Vector2(1, 1) * newSize;
    }

    public void ShowCrosshair()
    {
        Show();
        Position = GetViewport().GetMousePosition();
        Input.SetMouseMode(Input.MouseModeEnum.Hidden);
    }

    public void HideCrosshair()
    {
        Hide();
        Input.SetMouseMode(Input.MouseModeEnum.Visible);
    }

    public partial class CrossHairRecoil(Crosshair crosshair) : Node
    {
        private Vector2 _accumilatedImpulse = Vector2.Zero;
        private float _impulseScale = 1;
        private Tween _impulseTweener;

        public void ApplyImpulse(Vector2 impulse)
        {
            const float easeReturn = 0.2f;
            _accumilatedImpulse += impulse;
            _impulseScale += 0.1f;
            _impulseScale = Math.Clamp(_impulseScale, 0.6f, 1f);

            crosshair.CrosshairSprite.Position = _accumilatedImpulse * _impulseScale;

            _impulseTweener?.Kill();
            _impulseTweener = crosshair.CreateTween();
            _impulseTweener
                .TweenProperty(crosshair.CrosshairSprite, "position", Vector2.Zero, easeReturn)
                .SetEase(Tween.EaseType.Out);
            _impulseTweener.TweenProperty(this, "_accumilatedImpulse", Vector2.Zero, easeReturn);
            _impulseTweener.TweenProperty(this, "_impulseScale", 0f, 0.6f);
        }
    }
}
