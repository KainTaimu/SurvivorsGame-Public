class_name PickupUiMainbody
extends VBoxContainer

@export var item_name_label: RichTextLabel
@export var item_description_label: RichTextLabel
@export var item_icon_rect: TextureRect
@export var item_rarity_label: RichTextLabel

@export var rarity_colors: Dictionary[Globals.Rarity, Color] = {
	Globals.Rarity.COMMON: Color.GRAY,
	Globals.Rarity.UNCOMMON: Color.GREEN,
	Globals.Rarity.RARE: Color.STEEL_BLUE,
	Globals.Rarity.EPIC: Color.PURPLE,
	Globals.Rarity.LEGENDARY: Color.GOLD,
	Globals.Rarity.UNOBTAINABLE: Color.RED,
}


func set_rarity(rarity: Globals.Rarity):
	var color := rarity_colors[rarity]
	color.a = 0.5
	item_rarity_label.push_bold()
	item_rarity_label.push_color(color)
	item_rarity_label.append_text(Globals.rarity_to_stringname(rarity).to_upper())
	item_rarity_label.pop()
	item_rarity_label.pop()
