using Game.Core.Interfaces;

namespace Game.Players;

public partial class Player : Node2D, IHittable, IDamagable
{
    [Export]
    public Character Character { get; private set; } = null!;

    public int Health
    {
        get => Character.CharacterStats.Health;
        set => Character.CharacterStats.Health = value;
    }

    public void HandleHit()
    {
        throw new NotImplementedException();
    }
}
