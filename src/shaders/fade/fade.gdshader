shader_type canvas_item;

uniform float amount: hint_range(0.0, 1.0);

void fragment() {
	COLOR.a *= amount;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
