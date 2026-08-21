extends CanvasLayer

@export var health_bar: ProgressBar
@export var health_damage_bar: ProgressBar
@export var enemies_alive_label: Label
@export var wave_progress_label: Label

var damage_bar_tween: Tween

@onready var player_stats: CharacterStatsAccessor = CharacterStatsAccessor.new()
@onready var wave_controller: EnemyWaveControllerAccessor = EnemyWaveControllerAccessor.new()


func _ready() -> void:
	var main_player: Player = GameWorldInstance.MainPlayer
	if main_player != null:
		GameWorldInstance.MainPlayer.connect("OnDamaged", on_player_damaged)


func _process(_delta: float) -> void:
	var alive_enemies = wave_controller.AliveEnemies
	if alive_enemies != null:
		enemies_alive_label.text = "Enemies: %s" % wave_controller.AliveEnemies

	var wc: EnemyWaveController = wave_controller.WaveController
	if wc != null:
		var progress: float = wc.CurrentWaveProgress
		var wave_idx: int = wc.CurrentWaveIndex
		wave_progress_label.text = "Wave %s: %s%%" % [wave_idx + 1, roundi((1 - progress) * 100)]


func on_player_damaged(_dmg) -> void:
	health_bar.value = player_stats.Health / float(player_stats.MaxHealth)

	_kill_damage_bar_tween()
	damage_bar_tween = create_tween()
	damage_bar_tween.tween_interval(0.5)
	damage_bar_tween \
			.tween_property(health_damage_bar, "value", health_bar.value, 0.1) \
			.set_ease(Tween.EASE_OUT) \
			.set_trans(Tween.TRANS_EXPO)
	damage_bar_tween.tween_callback(_kill_damage_bar_tween)


func _kill_damage_bar_tween():
	if damage_bar_tween == null:
		return
	damage_bar_tween.kill()
	damage_bar_tween = null
