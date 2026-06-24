using Godot.Collections;

namespace Game.Players.Controllers;

public enum StatusEffectTags
{
	Unspecified,
	Damage,
	Slow,
}

public enum StatusEffectCause
{
	NotSpecified,
	Unknown,
	Weapon,
	Environment,
	Enemy,
}

[GlobalClass]
public partial class StatusEffect : Resource
{
	[Export]
	public required string Name;

	[Export]
	public StatusEffectCause Cause = StatusEffectCause.NotSpecified;

	public Guid Id => _id;
	private Guid _id;

	[Export]
	public required float InitialDuration;

	[Export]
	public bool Permanent;

	[Export]
	public Texture2D Icon = new PlaceholderTexture2D { Size = Vector2.One * 32 };

	[Export]
	public required Array<StatusEffectTags> Tags = [];

	[Export]
	public required Array<StatusEffectModifier> Modifiers = [];

	public float Duration { get; private set; }

	public void Process(float delta)
	{
		Duration -= delta;
	}

	public void Initialize()
	{
		_id = Guid.CreateVersion7();
		if (Permanent)
			Duration = -1;
		else
			Duration = InitialDuration;
	}

	public override string ToString()
	{
		return $"{{Name: {Name}, Id: {_id}, InitialDuration: {InitialDuration}, Duration: {Duration},"
			+ $" Tags: [{string.Join(", ", Tags)}], Modifiers: [{string.Join(", ", Modifiers)}]}}";
	}
}
