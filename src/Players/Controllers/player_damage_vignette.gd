extends CanvasLayer

@export var max_alpha: float = 0.12
@export var flash_duration: float = 0.4

var tween: Tween

@onready var vignette_rect: ColorRect = $ColorRect


func on_player_damaged() -> void:
	if tween != null:
		tween.kill()

	tween = create_tween()
	tween.tween_method(_set_vignette_alpha, max_alpha, 0, flash_duration)


func _set_vignette_alpha(value: float):
	var shader := vignette_rect.material as ShaderMaterial
	shader.set_shader_parameter("alpha", value)
