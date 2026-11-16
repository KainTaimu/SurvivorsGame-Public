@tool
class_name DeathScreen
extends CanvasLayer

signal on_death_screen_show


func _ready() -> void:
	if Engine.is_editor_hint():
		visible = false
		return
	var p: Player = GameWorldInstance.MainPlayer
	if not p.has_signal("OnPlayerDeath"):
		CustomLogger.log_error("expected player to have OnDeath signal")
		return
	p.connect("OnPlayerDeath", show_death_screen)


func _exit_tree() -> void:
	if Engine.is_editor_hint():
		return
	PauseController.Unpause(self)
	PauseController.Unlock(self)


func show_death_screen():
	show()
	PauseController.Lock(self)
	PauseController.Pause(self)
	on_death_screen_show.emit()


func _restart_game():
	get_tree().reload_current_scene()
