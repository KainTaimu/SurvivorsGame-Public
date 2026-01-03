shader_type canvas_item;

uniform vec2 frames = vec2(3.0, 1.0); // 3 horizontal, 1 vertical
varying vec4 custom_data;

// custom_data:
// x: flash
// y: frame index
// z: flip
// w: opacity
void fragment() {
	vec2 uv = UV;
	if (custom_data.z > 0.0) {
		uv.x = 1.0 - UV.x;
	}

	vec4 pixel_color = texture(TEXTURE, uv);
	COLOR = mix(pixel_color, vec4(1,1,1,1), custom_data.x);
	COLOR.a = pixel_color.a * custom_data.w;
}

void vertex() {
    custom_data = INSTANCE_CUSTOM;

    // Calculate UV offset
    float frame_index = floor(custom_data.y); // Use .y for frame index
    vec2 frame_size = 1.0 / frames;
    UV.x = (UV.x + mod(frame_index, frames.x)) * frame_size.x;
	UV.y = 1.0 - UV.y;
    //UV.y = (UV.y + floor(frame_index / frames.x)) * frame_size.y;
}