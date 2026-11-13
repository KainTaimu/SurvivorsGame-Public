// Copyright (C) 2026 Erwin Panergo
//
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Game.Levels.Controllers;

namespace Game.Players;

/// <summary>
/// A bridge that exposes CharacterStats properties for GDScript scripts to use
/// </summary>
[GlobalClass]
public partial class CharacterStatsAccessor : RefCounted
{
	private static CharacterStats Stats => GameWorld.Instance.MainPlayer.Character.CharacterStats;

	public int Health => Stats.Health;

	public int MaxHealth => Stats.MaxHealth;

	public float MoveSpeed => Stats.MoveSpeed;

	public float RunSpeed => Stats.RunSpeed;

	public float Stamina => Stats.Stamina;

	public float MaxStamina => Stats.MaxStamina;

	public int Defense => Stats.Defense;

	public float PickupRangeRadius => Stats.PickupRangeRadius;

	public int HealthRegenPerSecond => Stats.HealthRegenPerSecond;

	public float InvincibilityTime => Stats.InvincibilityTime;

	public float HitboxRadius => Stats.HitboxRadius;

	public float CriticalChance => Stats.CriticalChance;

	public float CriticalDamage => Stats.CriticalDamage;

	public float AttackSpeed => Stats.AttackSpeed;

	public float BloomScale => Stats.BloomScale;

	public float RecoilScale => Stats.RecoilScale;

	public float IncomingDamage => Stats.IncomingDamage;

	public float OutgoingDamage => Stats.OutgoingDamage;
}
