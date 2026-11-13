// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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
	public bool Hidden;

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
