using Game.Core;
using Game.Enemies;
using Godot.Collections;

namespace Game.Levels.Controllers;

[GlobalClass]
public partial class EnemyBlueprint : Resource
{
	[Export]
	public string Name { get; private set; } = null!;

	[Export]
	public string SpriteName { get; private set; } = null!;

	[Export]
	public EnemyType Type { get; private set; }

	[Export]
	public Array<AbstractEnemyBehavior> EnemyBehaviors = [];

	[Export]
	public EnemyStats Stats { get; private set; } = null!;
}
