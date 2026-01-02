using Game.Models;

namespace Game.Core.Interfaces;

public interface IHittable
{
    EntityType EntityType { get; }
    void HandleHit(DamageEvent damageEvent);
}
