namespace Game.Core.ECS;

public record struct DyingMarkerComponent(float TimeLeftUntilDestroy)
{
	public static readonly DyingMarkerComponent Default = new(0.2f);
}
