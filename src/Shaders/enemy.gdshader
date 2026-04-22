shader_type canvas_item;

const uint BYTE_MASK = 0xFFu;

varying float v_flip;
varying float v_opacity;
varying float v_flash;

void unpack_r(float packed, out float flip, out float opacity, out float flash) {
	uint bits = floatBitsToUint(packed);
	flip = float(bits & 1u);
	opacity = float((bits >> 1u) & BYTE_MASK) / 255.0;
	flash = float((bits >> 9u) & BYTE_MASK) / 255.0;
}

void unpack_b(float packed, out vec2 frame_count, out vec2 frame_index) {
	uint bits = floatBitsToUint(packed);
	uint frame_x = (bits >> 0u) & BYTE_MASK;
	uint frame_y = (bits >> 8u) & BYTE_MASK;
	uint frame_idx_x = (bits >> 16u) & BYTE_MASK;
	uint frame_idx_y = (bits >> 24u) & BYTE_MASK;

	frame_count = vec2(float(max(frame_x, 1u)), float(max(frame_y, 1u)));
	frame_index = vec2(float(frame_idx_x), float(frame_idx_y));
}

void unpack_a(float packed, out vec2 frame_size_px) {
	uint bits = floatBitsToUint(packed);
	uint frame_size_x = (bits >> 0u) & BYTE_MASK;
	uint frame_size_y = (bits >> 8u) & BYTE_MASK;
	frame_size_px = vec2(float(frame_size_x), float(frame_size_y));
}

void fragment() {
	vec2 uv = UV;
	if (v_flip > 0.5) {
		uv.x = 1.0 - UV.x;
	}

	vec4 pixel_color = texture(TEXTURE, uv);
	COLOR = mix(pixel_color, vec4(1.0, 1.0, 1.0, 1.0), v_flash);
	COLOR.a = pixel_color.a * v_opacity;
}

void vertex() {
	vec4 custom_data = INSTANCE_CUSTOM;

	vec2 frame_count;
	vec2 frame_index;
	vec2 frame_size_px;
	float flip;
	float opacity;
	float flash;
	unpack_r(custom_data.x, flip, opacity, flash);
	unpack_b(custom_data.z, frame_count, frame_index);
	unpack_a(custom_data.w, frame_size_px);
	v_flip = flip;
	v_opacity = opacity;
	v_flash = flash;

	vec2 frame_uv_size = 1.0 / frame_count;
	UV.x = (UV.x + frame_index.x) * frame_uv_size.x;
	UV.y = (1.0 - UV.y + frame_index.y) * frame_uv_size.y;
}
