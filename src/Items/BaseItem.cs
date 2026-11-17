// Copyright 2026 Erwin Panergo
// SPDX-License-Identifier: GPL-3.0-or-later

using Game.Levels.Controllers;
using Game.Players;

namespace Game.Items;

[GlobalClass]
public abstract partial class BaseItem : Node
{
	[Signal]
	public delegate void OnEquippedEventHandler();

	[Signal]
	public delegate void OnUnequippedEventHandler();

	public static Player Player => GameWorld.Instance.MainPlayer;

	public static CharacterStats PlayerStats => GameWorld.Instance.MainPlayer.Character.CharacterStats;

	public virtual void Process(double delta) { }
}
