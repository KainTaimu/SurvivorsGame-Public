class_name Globals

enum Rarity { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY, UNOBTAINABLE }

const RARITY_NAMES := ["Common", "Uncommon", "Rare", "Epic", "Legendary", "Unobtainable"]


static func rarity_to_stringname(rarity: Rarity) -> StringName:
	return RARITY_NAMES[rarity]
