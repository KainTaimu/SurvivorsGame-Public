extends Node


func _ready() -> void:
	Console.pause_enabled = true
	Console.remove_command("pause")
	Console.remove_command("unpause")
	Console.add_command("perf", _toggle_perf_monitor, [], 0, "Toggle performance monitor.")
	Console.add_command("god", _toggle_god, [], 0, "Toggle god mode.")


func _toggle_perf_monitor():
	var m = get_tree().root.find_child("PerformanceMonitor", true, false) as PerformanceMonitor
	if m == null:
		Console.print_error("PerformanceMonitor does not exist")
		return
	m.visible = not m.visible


func _toggle_god():
	var p: Player = GameWorldInstance.MainPlayer
	if p == null:
		Console.print_error("player does not exist")
		return
	var stats := p.Character.CharacterStats as CharacterStats
	var god: bool = stats.get("_godMode")
	stats.set("_godMode", !god)
	CustomLogger.log_info("god: %s" % !god)
	Console.print_line("god: %s" % !god)
