shader_type canvas_item;

varying vec4 custom_data;

void vertex() {
	custom_data = INSTANCE_CUSTOM;
}

// custom_data:
// x: flash
// y: NONE
// z: NONE
// w: opacity
void fragment() {	
	vec4 pixel_color = texture(TEXTURE, UV);
	COLOR = mix(pixel_color, vec4(1,1,1,1), custom_data.x);
	COLOR.a = pixel_color.a * custom_data.w;
}
