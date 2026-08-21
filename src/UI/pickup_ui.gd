class_name PickupUi
extends Node

@export var show_on_start: bool = false
@export var show_on_start_item_count: int = 3
@export var show_on_start_rarity_gate: Globals.Rarity = Globals.Rarity.COMMON

@export_group("Internal")
@export var grid_container: GridContainer
@export var weapon_registry: Registry = preload("uid://cafl4rhi4lyju")
@export var showcase_scene: PackedScene = preload("uid://c0svlhjww3qot")

var showcases: Array[ItemShowcase] = []
var weapons_picked: Array[OffensiveRegistryEntry] = []


func _ready() -> void:
	if show_on_start:
		_initialize_showcases(show_on_start_item_count)
		show_ui.call_deferred(show_on_start_item_count, show_on_start_rarity_gate)


func show_ui(item_limit: int, rarity_gate: Globals.Rarity) -> void:
	PauseController.Lock(self)
	PauseController.Pause(self)
	var all := weapon_registry.load_all_blocking()
	var weapons := all.values() as Array[Resource]

	var gated: Array[Resource] = []
	for w in weapons:
		if w.rarity <= rarity_gate:
			gated.append(w)
	weapons = gated
	item_limit = min(item_limit, len(weapons))
	_initialize_showcases(item_limit)

	for showcase in showcases:
		if len(weapons_picked) == len(weapons):
			return
		var entry := _weighted_pick(weapons) as OffensiveRegistryEntry
		if entry == null:
			CustomLogger.log_error("expected OffensiveRegistryEntry. got %s" % typeof(entry))
			continue

		var stats := _get_property_from_scene(entry.scene, "Stats") as BaseItemStats
		if stats == null:
			CustomLogger.log_error("expected BaseItemStats. got %s" % typeof(stats))
			continue
		var props := _get_property_from_scene(entry.scene, "Properties") as BaseItemProperties
		if props == null:
			CustomLogger.log_error("expected BaseItemProperties. got %s" % typeof(props))

		# BUG: This sometimes causes an invalid property access crash
		showcase.assign_item.call_deferred(entry.scene, props, stats)
		item_limit -= 1
		if item_limit == 0:
			return


func _exit_tree() -> void:
	PauseController.Unlock(self)
	PauseController.Unpause(self)


func exit() -> void:
	queue_free()


func _initialize_showcases(item_count: int) -> void:
	var columns := maxi(1, grid_container.columns)
	var rows := maxi(1, ceili(float(item_count) / columns))
	var total := rows * columns

	while showcases.size() > total:
		var extra: ItemShowcase = showcases.pop_back()
		extra.queue_free()

	for i in range(showcases.size(), total):
		var showcase := showcase_scene.instantiate() as ItemShowcase
		grid_container.add_child(showcase)
		showcases.append(showcase)
		showcase.on_item_picked.connect(_on_item_picked)


func _on_item_picked(picked_scene: PackedScene) -> void:
	var player: Player = GameWorldInstance.MainPlayer
	if player == null:
		CustomLogger.log_error("cannot give item. GameWorldInstance.MainPlayer is null")
		return
	var wpn_controller: Node = player.WeaponController
	wpn_controller.add_child(picked_scene.instantiate())
	queue_free()


func _weighted_pick(weapons: Array[Resource]) -> OffensiveRegistryEntry:
	var picked: OffensiveRegistryEntry = null

	var eligible: Array[OffensiveRegistryEntry] = []
	var total := 0.0
	for w in weapons:
		var weapon := w as OffensiveRegistryEntry
		if weapon == null or w in weapons_picked:
			continue
		eligible.append(weapon)
		total += weapon.spawn_weight

	if total <= 0.0:
		if eligible.is_empty():
			return null
		picked = eligible[randi() % eligible.size()]
		weapons_picked.append(picked)
		return picked

	var r := randf() * total
	var cumulative := 0.0
	for weapon in eligible:
		cumulative += weapon.spawn_weight
		if r < cumulative:
			weapons_picked.append(weapon)
			return weapon

	picked = eligible[eligible.size() - 1]
	weapons_picked.append(picked)
	return picked


static func _get_property_from_scene(scene: PackedScene, property_name: String) -> Variant:
	var state := scene.get_state()
	var node_idx := 0

	# Find property by name
	var prop_count := state.get_node_property_count(node_idx)
	for i in range(prop_count):
		if state.get_node_property_name(node_idx, i) == property_name:
			return state.get_node_property_value(node_idx, i)

	return null
