using System.Collections.Generic;

namespace Game.Models;

public readonly struct DamageEvent
{
    public required int Damage { get; init; }
    public Queue<IEffect> Effects { get; init; }
}
