using Godot.Collections;

namespace Game.Players.Controllers;

public enum StatusEffectTags
{
	Unspecified,
	Damage,
	Slow,
}

[GlobalClass]
public partial class StatusEffect : Resource
{
	[Export]
	public required string Name;

	public int Id;

	[Export]
	public required float InitialDuration;

	[Export]
	public required Array<StatusEffectTags> Tags = [];

	[Export]
	public required Array<StatusEffectModifier> Modifiers = [];

	public float Duration;

	public override string ToString()
	{
		return $"{{Name: {Name}, Id: {Id}, InitialDuration: {InitialDuration}, Duration: {Duration},"
			+ $" Tags: [{string.Join(", ", Tags)}], Modifiers: [{string.Join(", ", Modifiers)}]}}";
	}
}
