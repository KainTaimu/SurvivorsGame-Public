class_name OffensiveRegistryEntry
extends Resource

@export var properties: BaseItemProperties
@export var scene: PackedScene
## Lower is rarer
@export_range(0, 1, 0.001) var spawn_weight: float
@export var rarity: Globals.Rarity
