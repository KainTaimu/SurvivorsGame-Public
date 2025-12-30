namespace Game.Core.Interfaces;

public interface IDamagable
{
    [Signal]
    delegate void OnHitEventHandler();

    int Health { get; set; }
}
