class_name ItemShowcase
extends MarginContainer

signal on_item_picked(scene: PackedScene)

var assigned_item_scene: PackedScene
var assigned_item_properties: BaseItemProperties
var is_mouse_inside: bool
var has_item_been_selected: bool
var has_item_been_assigned: bool

@export var item_name_label: RichTextLabel
@export var item_description_label: RichTextLabel
@export var panel: PanelContainer
@export var item_icon_rect: TextureRect
@export var unassigned_texture: TextureRect
@export var main_body: Control
@export var highlight_sfx: AudioStreamPlayer


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


func assign_item(scene: PackedScene, properties: BaseItemProperties, _stats: BaseItemStats) -> void:
	item_name_label.text = "[b]%s[/b]" % properties.Name
	item_description_label.text = properties.Description
	item_icon_rect.texture = properties.ItemIcon
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
