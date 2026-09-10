@tool
class_name DeathScreen
extends CanvasLayer

signal on_death_screen_show


func _ready() -> void:
	if Engine.is_editor_hint():
		visible = false
		return


func _exit_tree() -> void:
	if Engine.is_editor_hint():
		return
	PauseController.Unlock(self)
	PauseController.Unpause(self)


func show_death_screen():
	show()
	PauseController.Lock(self)
	PauseController.Pause(self)
	on_death_screen_show.emit()


func _restart_game():
	get_tree().reload_current_scene()
