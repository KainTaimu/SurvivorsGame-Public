shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;
uniform float edge_width = 0.04;
uniform float refraction_strength = 0.03;
uniform float chromatic_aberration = 0.004;

instance uniform vec2 point = vec2(0.5, 0.5);
instance uniform float radius = 0.2;
instance uniform float feather = 0.01;

void fragment() {
	float aspect = SCREEN_PIXEL_SIZE.y / SCREEN_PIXEL_SIZE.x;
	vec2 diff = SCREEN_UV - point;
	diff.x *= aspect;

	float dist = length(diff);
	vec2 dir = dist > 0.0001 ? diff / dist : vec2(0.0);
	vec2 uv_dir = vec2(dir.x / aspect, dir.y);

	float rim_start = max(radius - edge_width, 0.0);
	float edge_factor = smoothstep(rim_start, max(radius, rim_start + 0.0001), dist);
	edge_factor *= edge_factor;

	vec2 refract_offset = -uv_dir * edge_factor * refraction_strength;
	vec2 ca_offset = uv_dir * edge_factor * chromatic_aberration;

	float r = texture(screen_texture, SCREEN_UV + refract_offset + ca_offset).r;
	float g = texture(screen_texture, SCREEN_UV + refract_offset).g;
	float b = texture(screen_texture, SCREEN_UV + refract_offset - ca_offset).b;
	vec3 glass_color = vec3(r, g, b);

	float mask = smoothstep(radius, radius + max(feather, 0.0001), dist);
	COLOR = vec4(mix(glass_color, vec3(0.0), mask), 1.0);
}