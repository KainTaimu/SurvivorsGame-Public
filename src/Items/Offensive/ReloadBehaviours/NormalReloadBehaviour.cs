namespace Game.Items.Offensive;

[Tool]
[GlobalClass]
public partial class NormalReloadBehaviour : AbstractReloadBehaviour
{
	public required Func<float> ReloadTime { get; set; }

	private float _timeUntilFinishedReloading;

	public override void Reload()
	{
		IsReloading = true;
		EmitSignalOnReloadStart();
		_timeUntilFinishedReloading = ReloadTime();
	}

	public override void Process(float delta)
	{
		if (!IsReloading)
			return;

		_timeUntilFinishedReloading -= delta;
		if (_timeUntilFinishedReloading > 0)
			return;

		EmitSignalOnReloadEnd();
		IsReloading = false;
	}
}
