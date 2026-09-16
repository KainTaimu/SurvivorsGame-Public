@tool
class_name HealthBar
extends PanelContainer

@export_range(0, 1, 0.01) var ratio: float:
	get:
		return ratio
	set(v):
		var old_value := ratio
		ratio = clampf(v, 0.0, 1.0)
		if v < old_value:
			_time_until_standby = 3
		queue_redraw()
@export var health_color: Color = Color.GREEN
@export var damage_color: Color = Color.RED
@export var background: Color = Color.BLACK
@export var a: int = 15
@export var b: int = 90
@export var m: float = 0.1

var _time_until_standby: float

var _real_rect: Rect2 = Rect2()
var _damage_rect: Rect2 = Rect2()

const E := 2.718281828459045235360287471352


func _draw() -> void:
	var health_size := Vector2(min(ratio * size.x, size.x), 8)
	_real_rect = Rect2(Vector2(0, 0), health_size)
	draw_rect(Rect2(Vector2.ZERO, Vector2(size.x, 8)), background)
	draw_rect(_damage_rect, damage_color, true)
	draw_rect(_real_rect, health_color, true)


func _process(delta: float) -> void:
	if _time_until_standby <= 0:
		return
	_time_until_standby = max(0, _time_until_standby - delta)
	_damage_rect = Rect2(Vector2.ZERO, _damage_rect.size.lerp(_real_rect.size, delta * 3))

	offset_transform_rotation = (E ** (-a * (3 - _time_until_standby))) * cos(b * (3 - _time_until_standby)) * m
	queue_redraw()

	if _time_until_standby <= 0:
		_damage_rect = _real_rect
