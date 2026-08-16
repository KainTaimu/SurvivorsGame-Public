extends PanelContainer

@export var limit: int = 3
@export var weapon_registry: Registry = preload("uid://cafl4rhi4lyju")

var showcases: Array[ItemShowcase] = []


func _ready() -> void:
	PauseController.Lock(self)
	PauseController.Pause(self)
	_initialize_showcases()
	var all := weapon_registry.load_all_blocking()
	var weapons := all.values() as Array[Resource]
	_shuffle_array(weapons)

	for showcase in showcases:
		var entry := weapons.pop_front() as OffensiveRegistryEntry
		if entry == null:
			CustomLogger.log_error("expected OffensiveRegistryEntry. got %s" % typeof(entry))
			continue
		var stats := _get_property_from_scene(entry.scene, "Stats") as BaseItemStats
		var props := _get_property_from_scene(entry.scene, "Properties") as BaseItemProperties
		showcase.assign_item(entry.scene, props, stats)
		limit -= 1
		if limit == 0:
			return


func _exit_tree() -> void:
	PauseController.Unlock(self)
	PauseController.Unpause(self)
	

func exit() -> void:
	queue_free()


func _initialize_showcases() -> int:
	var count := 0
	var grid_container: Control = $CenterContainer/GridContainer
	for child in grid_container.get_children():
		var showcase := child as ItemShowcase
		if showcase == null:
			continue
		count += 1
		showcases.append(showcase)
		showcase.on_item_picked.connect(_on_item_picked)
	return count


func _on_item_picked(picked_scene: PackedScene) -> void:
	var player: Player = GameWorldInstance.MainPlayer
	if player == null:
		CustomLogger.log_error("cannot give item. GameWorldInstance.MainPlayer is null")
		return
	var wpn_controller: Node = player.WeaponController
	wpn_controller.add_child(picked_scene.instantiate())
	queue_free()


static func _shuffle_array(arr: Array):
	var n := len(arr)
	while n > 1:
		var k := randi_range(0, n) % n
		n -= 1
		var tmp = arr[n]
		arr[n] = arr[k]
		arr[k] = tmp


static func _get_property_from_scene(scene: PackedScene, property_name: String) -> Variant:
	var state := scene.get_state()
	var node_idx := 0

	# Find property by name
	var prop_count := state.get_node_property_count(node_idx)
	for i in range(prop_count):
		if state.get_node_property_name(node_idx, i) == property_name:
			return state.get_node_property_value(node_idx, i)

	return null
