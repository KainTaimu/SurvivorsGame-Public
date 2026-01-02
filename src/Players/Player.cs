using Game.Core;
using Game.Core.Interfaces;
using Game.Models;

namespace Game.Players;

public partial class Player : Node2D, IHittable
{
    [Export]
    public Character Character { get; private set; } = null!;

    public EntityType EntityType => EntityType.Player;

    // TODO: Should Players be affected by effects?
    public void HandleHit(DamageEvent damageEvent)
    {
        var stats = Character.CharacterStats;

        stats.Health -= damageEvent.Damage;
    }
}
