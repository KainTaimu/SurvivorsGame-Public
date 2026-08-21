extends Node

@export var pickup_scene: PackedScene


func on_enemy_wave_controller_wave_end() -> void:
	_create_pickup_ui.call_deferred()


func _create_pickup_ui():
	var pc: PickupUi = pickup_scene.instantiate()
	pc.show_ui(9)
	add_child(pc)
