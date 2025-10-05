using System.Collections.Generic;
using Newtonsoft.Json;
using SurvivorsGame.Items.Passive;

namespace SurvivorsGame.Entities.Characters;

[JsonObject(MemberSerialization.OptIn)]
public partial class PlayerStats : Node
{
    private Player _player;

    private List<BasePassive> _playerItems;

    private PlayerPassiveController _playerPassiveController;

    [ExportCategory("Main attributes")]
    [JsonProperty]
    public int Health { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public int MaxHealth { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float MoveSpeed { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float Defense { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float CriticalChance { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float PickupRange { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float HealthRegenPerSecond { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float InvincibilityTime { get; set; }

    [ExportCategory("Multiplier attributes")]
    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float HealthMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float MoveSpeedMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float IncomingDamageMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float OutgoingDamageMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float CriticalChanceMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float CriticalDamageMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float AttackSpeedMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float ProjectileMultiplier { get; set; }

    [Export(PropertyHint.Range, "0,0,or_greater,hide_slider")]
    [JsonProperty]
    public float XpMultiplier { get; set; }

    public Player Player
    {
        get => _player;
        set
        {
            if (_player is null)
            {
                return;
            }

            _player = value;
            _playerPassiveController = _player.PassiveController;
            _playerItems = _playerPassiveController.CurrentPassives;
        }
    }

    private int GetMaxHealth()
    {
        var sum = 0;
        foreach (var item in _playerItems)
        {
            sum += item.Stats.Health;
            sum *= (int)item.Stats.StatMultipliers.HealthMultiplier;
        }

        return MaxHealth + sum;
    }
}