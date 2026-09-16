extends Camera2D

signal on_current_zoom_changed(newValue: float, oldValue: float)

@export var current_zoom: float = 0.65:
	get:
		return current_zoom
	set(v):
		if is_equal_approx(current_zoom, v):
			return
		var old := current_zoom
		current_zoom = v
		zoom = Vector2.ONE * v
		on_current_zoom_changed.emit(v, old)

@export var min_zoom := 0.15
@export var max_zoom := 0.9

@export var _damage_shake_scale: float = 0.1

var _shake: float
const a := 15
const b := 90

const E := 2.718281828459045235360287471352


func _process(delta: float) -> void:
	if _shake <= 0:
		return
	_shake = max(0, _shake - delta)
	rotation = (E ** (-a * (3 - _shake))) * cos(b * (3 - _shake)) * _damage_shake_scale * randf_range(0.5, 1.0)
	rotation *= 1 if randf() < 0.5 else -1


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("ZOOM_IN"):
		current_zoom += 0.05
	elif event.is_action_pressed("ZOOM_OUT"):
		current_zoom -= 0.05
	current_zoom = clampf(current_zoom, min_zoom, max_zoom)


func on_player_damaged():
	_shake = 3
