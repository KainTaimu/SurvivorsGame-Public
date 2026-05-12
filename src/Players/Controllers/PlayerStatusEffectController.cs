using System.Collections.Generic;
using System.Reflection;

namespace Game.Players.Controllers;

public partial class PlayerStatusEffectController : Node
{
	[Signal]
	public delegate void OnStatusEffectAppliedEventHandler();

	[Export]
	public Player Player = null!;

	[Export]
	private Godot.Collections.Array<StatusEffect> _testEffect = [];

	private CharacterStats Stats => Player.Character.CharacterStats;

	private readonly Godot.Collections.Array<StatusEffect> _activeEffects = [];

	private readonly Dictionary<CharacterStatType, AbstractStat> _statToStatField = [];

	public override void _Ready()
	{
		InitializeStatStacks();

#if DEBUG
		foreach (var statusEffect in _testEffect)
			AddStatusEffect(statusEffect);
#endif
	}

	public override void _Process(double delta)
	{
		var toRemove = new List<StatusEffect>();
		foreach (var activeEffect in _activeEffects)
		{
			activeEffect.Duration -= (float)delta;
			if (activeEffect.Duration <= 0)
				toRemove.Add(activeEffect);
		}

		foreach (var statusEffect in toRemove)
			RemoveStatusEffect(statusEffect);
	}

	private void RemoveStatusEffect(StatusEffect statusEffect)
	{
		foreach (var modifier in statusEffect.Modifiers)
		{
			var targetStat = _statToStatField[modifier.StatName];
			switch (modifier.Operation)
			{
				case StatusEffectModifier.StatusEffectModifierOperation.Add:
					targetStat.Flat.Remove(modifier.Value);
					break;
				case StatusEffectModifier.StatusEffectModifierOperation.Multiply:
					targetStat.Multipliers.Remove(modifier.Value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		_activeEffects.Remove(statusEffect);
	}

	public void AddStatusEffect(StatusEffect statusEffect)
	{
		statusEffect.Duration = statusEffect.InitialDuration;
		foreach (var modifier in statusEffect.Modifiers)
		{
			if (!_statToStatField.TryGetValue(modifier.StatName, out var targetStat))
			{
				Logger.LogError("Unknown stat name");
				continue;
			}
			switch (modifier.Operation)
			{
				case StatusEffectModifier.StatusEffectModifierOperation.Add:
					targetStat.Flat.Add(modifier.Value);
					break;
				case StatusEffectModifier.StatusEffectModifierOperation.Multiply:
					targetStat.Multipliers.Add(modifier.Value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			Logger.LogInfo($"New ST: {statusEffect}");
		}
		_activeEffects.Add(statusEffect);
	}

	private void InitializeStatStacks()
	{
		const BindingFlags flags =
			BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase;
		var pType = Stats.GetType();
		var properties = pType.GetFields(flags);
		foreach (var field in properties)
		{
			var value = field.GetValue(Stats);
			if (value is null)
				continue;

			var statEnum = field.Name switch
			{
				"_health" => CharacterStatType.Health,
				"_maxHealth" => CharacterStatType.MaxHealth,
				"_moveSpeed" => CharacterStatType.MoveSpeed,
				"_defense" => CharacterStatType.Defense,
				"_criticalChance" => CharacterStatType.CriticalChance,
				"_pickupRangeRadius" => CharacterStatType.PickupRangeRadius,
				"_healthRegenPerSecond" => CharacterStatType.HealthRegenPerSecond,
				"_invincibilityTime" => CharacterStatType.InvincibilityTime,
				"_hitboxRadius" => CharacterStatType.HitboxRadius,
				"_incomingDamageMultiplier" => CharacterStatType.IncomingDamageMultiplier,
				"_outgoingDamageMultiplier" => CharacterStatType.OutgoingDamageMultiplier,
				"_criticalChanceMultiplier" => CharacterStatType.CriticalChanceMultiplier,
				"_criticalDamageMultiplier" => CharacterStatType.CriticalDamageMultiplier,
				"_attackSpeedMultiplier" => CharacterStatType.AttackSpeedMultiplier,
				"_projectileMultiplier" => CharacterStatType.ProjectileMultiplier,
				"_xpMultiplier" => CharacterStatType.XpMultiplier,
				_ => throw new ArgumentOutOfRangeException(),
			};

			try
			{
				_statToStatField[statEnum] = (AbstractStat)value;
			}
			catch (InvalidCastException) { }
		}
	}
}
