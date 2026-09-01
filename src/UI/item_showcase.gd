class_name ItemShowcase
extends MarginContainer

signal on_item_picked(scene: PackedScene)

var assigned_item_scene: PackedScene
var assigned_item_properties: BaseItemProperties
var is_mouse_inside: bool
var has_item_been_selected: bool
var has_item_been_assigned: bool

@export var panel: PanelContainer
@export var unassigned_texture: TextureRect
@export var highlight_sfx: AudioStreamPlayer
@export var main_body_scene: PackedScene

var main_body: PickupUiMainbody


func _gui_input(event: InputEvent) -> void:
	if not has_item_been_assigned:
		return
	if has_item_been_selected:
		return
	if not is_mouse_inside:
		return

	var mouse := event as InputEventMouseButton
	if mouse == null:
		return
	if mouse.button_index != MOUSE_BUTTON_LEFT:
		return
	if not mouse.is_released():
		return

	on_item_picked.emit(assigned_item_scene)


func assign_item(scene: PackedScene, properties: BaseItemProperties, _stats: BaseItemStats, rarity: Globals.Rarity) -> void:
	main_body = main_body_scene.instantiate() as PickupUiMainbody
	add_child(main_body)

	main_body.item_name_label.text = "[b]%s[/b]" % properties.Name
	main_body.item_description_label.text = properties.Description
	main_body.item_icon_rect.texture = properties.ItemIcon
	main_body.set_rarity(rarity)
	assigned_item_scene = scene
	assigned_item_properties = properties
	_set_assigned_state(true)


func reset() -> void:
	assigned_item_scene = null
	assigned_item_properties = null
	has_item_been_selected = false
	has_item_been_assigned = false
	hide()
	highlight(false)


func highlight(flag: bool) -> void:
	panel.self_modulate = Color.WHITE if flag else Color.TRANSPARENT


func _set_mouse_inside(inside: bool) -> void:
	if not has_item_been_assigned:
		return
	is_mouse_inside = inside
	mouse_default_cursor_shape = (
			CursorShape.CURSOR_POINTING_HAND
			if is_mouse_inside
			else CursorShape.CURSOR_ARROW
	)

	highlight(inside)
	if inside:
		highlight_sfx.play()


func _set_assigned_state(set_as_assigned: bool) -> void:
	has_item_been_assigned = set_as_assigned
	if set_as_assigned:
		main_body.show()
		unassigned_texture.hide()
	else:
		main_body.hide()
		unassigned_texture.show()


func _on_mouse_entered() -> void:
	_set_mouse_inside(true)


func _on_mouse_exited() -> void:
	_set_mouse_inside(false)
