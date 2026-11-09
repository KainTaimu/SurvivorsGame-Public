@tool
extends Node

@export var do_rename: bool:
	get:
		return do_rename
	set(v):
		if ! v:
			return
		v = false
		_do_rename()


func _do_rename():
	var i := 1
	for child in get_children():
		var wave := child as AbstractWave
		if wave == null:
			continue
		wave.name = str(hash(wave.name))

	for child in get_children():
		var wave := child as AbstractWave
		if wave == null:
			continue
		var script := wave.get_script() as Script
		var wave_name = " " + wave.WaveName if not wave.WaveName.is_empty() else ""
		wave.name = "%s%s %s" % [i, wave_name, script.get_global_name().substr(4)]
		i += 1
