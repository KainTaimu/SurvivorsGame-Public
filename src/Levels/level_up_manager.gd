extends Node

@export var pickup_scene: PackedScene


func on_enemy_wave_controller_wave_start(wave: AbstractWave) -> void:
	_create_pickup_ui.call_deferred()


func _create_pickup_ui():
	var pc := pickup_scene.instantiate()
	add_child(pc)
