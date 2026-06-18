shader_type canvas_item;

uniform float u_color_factor : hint_range(0.0, 1.0) = 1.0;

void fragment() {
    vec4 sample = texture(TEXTURE, UV);
    float grey = 0.21 * sample.r + 0.71 * sample.g + 0.07 * sample.b;
    COLOR = vec4(
        mix(grey, sample.r, u_color_factor),
        mix(grey, sample.g, u_color_factor),
        mix(grey, sample.b, u_color_factor),
        sample.a
    );
}