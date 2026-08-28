extends Node2D

@export var pickup_scene: PackedScene

var pickup_ui: PickupUi


func _input(event: InputEvent) -> void:
	if not event.is_action_pressed("SHOW_GIVE_CHEAT"):
		return
	if pickup_ui != null:
		pickup_ui.exit()
		_on_pickup_ui_closed()
		return
	_show_pickup()


func _show_pickup() -> void:
	if pickup_ui != null:
		return

	pickup_ui = pickup_scene.instantiate() as PickupUi
	pickup_ui.on_complete.connect(_on_pickup_ui_closed)

	add_child(pickup_ui)
	pickup_ui.show_ui(999, Globals.Rarity.COMMON, Globals.Rarity.UNOBTAINABLE)


func _on_pickup_ui_closed() -> void:
	pickup_ui = null
