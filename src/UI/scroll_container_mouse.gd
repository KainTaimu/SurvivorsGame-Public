class_name ScrollContainerMouse
extends ScrollContainer

@export var scroll_area: Control

@export_range(0, 1) var ratio: float:
	get:
		return ratio
	set(value):
		ratio = value
		call_deferred("_set_ratio")

@export_range(0, 1, 0.025) var snap_threshold: float = 0.1

@onready var v_scroll := get_v_scroll_bar()


func _process(_delta: float) -> void:
	var mouse_pos := get_local_mouse_position()
	if not get_rect().has_point(mouse_pos):
		return
	ratio = mouse_pos.y / size.y
	if ratio < snap_threshold:
		ratio = 0
	elif ratio > 1.0 - snap_threshold:
		ratio = 1


func _set_ratio():
	v_scroll.ratio = ratio
