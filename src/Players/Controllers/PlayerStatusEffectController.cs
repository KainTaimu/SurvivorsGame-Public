using System.Collections.Generic;
using System.Reflection;
using Godot.Collections;

namespace Game.Players.Controllers;

public partial class PlayerStatusEffectController : Node
{
	[Signal]
	public delegate void OnStatusEffectAppliedEventHandler(StatusEffect statusEffect);

	[Signal]
	public delegate void OnStatusEffectRemovedEventHandler(StatusEffect statusEffect);

	[Export]
	public Player Player = null!;

	[Export]
	private Array<StatusEffect> _testEffect = [];

	private CharacterStats Stats => Player.Character.CharacterStats;

	private readonly Array<StatusEffect> _activeEffects = [];

	private readonly System.Collections.Generic.Dictionary<CharacterStatType, AbstractStat> _statToStatField = [];

	public override void _Ready()
	{
		InitializeStatStacks();

#if DEBUG
		foreach (var statusEffect in _testEffect)
			CallDeferred(MethodName.AddStatusEffect, statusEffect);
#endif
	}

	public override void _Process(double delta)
	{
		var toRemove = new List<StatusEffect>();
		foreach (var activeEffect in _activeEffects)
		{
			if (activeEffect.Permanent)
				continue;

			activeEffect.Process((float)delta);
			if (activeEffect.Duration <= 0)
				toRemove.Add(activeEffect);
		}

		foreach (var statusEffect in toRemove)
			RemoveStatusEffect(statusEffect);
	}

	public void RemoveStatusEffect(StatusEffect statusEffect)
	{
		if (!_activeEffects.Contains(statusEffect))
		{
			Logger.LogError($"Attempt to remove status effect {statusEffect.Name} that has not been applied.");
			return;
		}

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
		Logger.LogDebug($"Player status effect removed: {statusEffect}");
		EmitSignalOnStatusEffectRemoved(statusEffect);
	}

	// TODO: Handle already existing status effect. Reset duration?
	public void AddStatusEffect(StatusEffect statusEffect)
	{
		statusEffect.Initialize();

		foreach (var modifier in statusEffect.Modifiers)
		{
			if (!_statToStatField.TryGetValue(modifier.StatName, out var targetStat))
			{
				Logger.LogError($"Unknown stat name {modifier.StatName}");
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
		}

		_activeEffects.Add(statusEffect);
		EmitSignalOnStatusEffectApplied(statusEffect);
		Logger.LogDebug($"New status effect applied to Player: {statusEffect}");
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
				"_bloomMultiplier" => CharacterStatType.BloomMultiplier,
				"_recoilMultiplier" => CharacterStatType.RecoilMultiplier,
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
