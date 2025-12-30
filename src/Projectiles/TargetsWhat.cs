namespace Game.Projectiles;

[Flags]
public enum TargetsWhat
{
    None = 0,
    Player = 1 << 0,
    Enemy = 1 << 1,
    Breakable = 1 << 2,
}
