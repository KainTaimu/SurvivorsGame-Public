// using Game.Models;
//
// namespace Game.Projectiles.Controllers;
//
// public partial class ProjectileGrid : Node
// {
//     private const int GRID_SIZE = 32;
// }
//
// public partial class Pistol(TargetsWhat targetsWhat) : Node, IWeapon
// {
//     public WeaponType WeaponType => WeaponType.Ranged;
//     public required TargetsWhat TargetsWhat = targetsWhat;
//
//     public void OnTargetHit()
//     {
//         throw new NotImplementedException();
//     }
// }
//
// public partial class Projectile : Node { }
//
// public interface IWeapon
// {
//     WeaponType WeaponType { get; }
//     TargetsWhat TargetsWhat { get; }
//
//     void OnTargetHit();
// }
//
// public enum WeaponType
// {
//     None,
//     Melee,
//     Ranged,
// }
//
// public interface IHittable
// {
//     void TakeHit();
// }
