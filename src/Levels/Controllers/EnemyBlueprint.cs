using Game.Core;
using Game.Players;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyBlueprint : Resource
{
	[Export]
	public string Name { get; private set; } = null!;

	[Export]
	public EntityType Type { get; private set; }

	[Export]
	public CharacterStats Stats { get; private set; } = null!;
}
