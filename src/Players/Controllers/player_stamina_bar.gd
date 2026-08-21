extends PanelContainer

@export var value: float:
	get:
		return value
	set(value):
		bar_left.visible = value < 1.0
		bar_right.visible = value < 1.0
		value = value
		bar_left.value = value
		bar_right.value = value
@export var bar_left: ProgressBar
@export var bar_right: ProgressBar

@onready var character_stats := CharacterStatsAccessor.new()


func _process(_delta: float) -> void:
	var stamina: float = character_stats.Stamina
	var max_stamina: float = character_stats.MaxStamina
	value = clampf(stamina / max_stamina, 0, 1)
