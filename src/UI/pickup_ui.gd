@tool
class_name PickupUi
extends CanvasLayer

signal on_complete

@export var show_on_start: bool = false
@export var show_on_start_item_count: int = 3
## Rarity is inclusive
@export var show_on_start_rarity_lower_gate: Globals.Rarity = Globals.Rarity.COMMON
## Rarity is inclusive
@export var show_on_start_rarity_upper_gate: Globals.Rarity = Globals.Rarity.COMMON

@export_group("Internal")
@export var grid_container: GridContainer
@export var item_registry: Registry = preload("uid://cafl4rhi4lyju")
@export var showcase_scene: PackedScene = preload("uid://c0svlhjww3qot")

var showcases: Array[ItemShowcase] = []
var items_picked: Array[ItemRegistryEntry] = []


func _ready() -> void:
	if Engine.is_editor_hint():
		visible = false
		return
	if show_on_start:
		_initialize_showcases(show_on_start_item_count)
		show_ui.call_deferred(
			show_on_start_item_count,
			show_on_start_rarity_lower_gate,
			show_on_start_rarity_upper_gate,
		)


func _input(event: InputEvent) -> void:
	if Engine.is_editor_hint():
		return
	if event.is_action_pressed("ui_cancel"):
		queue_free()


func show_ui(
		item_limit: int,
		rarity_lower_gate: Globals.Rarity,
		rarity_upper_gate: Globals.Rarity,
) -> void:
	show()
	PauseController.Lock(self)
	PauseController.Pause(self)
	var all := item_registry.load_all_blocking()
	var items := all.values() as Array[Resource]

	var gated: Array[Resource] = []
	for i in items:
		if i.rarity >= rarity_lower_gate && i.rarity <= rarity_upper_gate:
			gated.append(i)
	assert(len(gated) > 0, "show_ui was called with no valid items after filtering by rarity.")
	items = gated
	item_limit = min(item_limit, len(items))
	_initialize_showcases(item_limit)

	_assign_item_recurse(showcases, items, item_limit)


func _assign_item_recurse(arr: Array[ItemShowcase], items: Array[Resource], remaining: int) -> void:
	if remaining == 0:
		return

	var showcase: ItemShowcase = arr.pop_front()
	if len(items_picked) == len(items):
		return
	var entry := _weighted_pick(items) as ItemRegistryEntry
	if entry == null:
		CustomLogger.log_error("expected OffensiveRegistryEntry. got %s" % typeof(entry))
		return

	# var stats := _get_property_from_scene(entry.scene, "Stats") as BaseItemStats
	# if stats == null:
	# 	CustomLogger.log_error("expected BaseItemStats. got %s" % typeof(stats))
	# 	return

	var props := entry.properties
	if props == null:
		CustomLogger.log_error("expected BaseItemProperties. got %s" % typeof(props))

	showcase.assign_item.call_deferred(entry.scene, props, entry.rarity)
	showcase.offset_transform_scale = Vector2.ONE * 1.04
	var tween := showcase.create_tween()
	tween.set_trans(Tween.TRANS_LINEAR).set_ease(Tween.EASE_IN)
	tween.tween_property(showcase, "offset_transform_scale", Vector2(1.0, 1.0), 0.2)

	get_tree().create_timer(0.025, true, false, true).timeout.connect(
		_assign_item_recurse.bind(
			arr,
			items,
			remaining -
			1,
		),
	)


func _exit_tree() -> void:
	if Engine.is_editor_hint():
		return
	PauseController.Unlock(self)
	PauseController.Unpause(self)


func exit() -> void:
	queue_free()


func _initialize_showcases(item_count: int) -> void:
	var columns := maxi(1, grid_container.columns)
	var rows := maxi(2, ceili(float(item_count) / columns))
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
	var item_controller: Node = player.ItemController
	item_controller.add_child(picked_scene.instantiate())
	on_complete.emit()
	queue_free()


func _weighted_pick(items: Array[Resource]) -> ItemRegistryEntry:
	var picked: ItemRegistryEntry = null

	var eligible: Array[ItemRegistryEntry] = []
	var total := 0.0
	for i in items:
		var item := i as ItemRegistryEntry
		if item == null or i in items_picked:
			continue
		eligible.append(item)
		total += item.spawn_weight

	if total <= 0.0:
		if eligible.is_empty():
			return null
		picked = eligible[randi() % eligible.size()]
		items_picked.append(picked)
		return picked

	var r := randf() * total
	var cumulative := 0.0
	for item in eligible:
		cumulative += item.spawn_weight
		if r < cumulative:
			items_picked.append(item)
			return item

	picked = eligible[eligible.size() - 1]
	items_picked.append(picked)
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
