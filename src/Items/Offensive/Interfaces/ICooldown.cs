namespace Game.Items.Offensive;

public interface ICooldown
{
	float CooldownDuration { get; set; }

	void Process(float delta);
}
