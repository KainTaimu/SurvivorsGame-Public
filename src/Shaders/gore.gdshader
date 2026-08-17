shader_type canvas_item;

// Accumulated gameplay seconds, driven from C# (not TIME) so particles
// freeze while the tree is paused.
uniform float game_time;

const uint BYTE_MASK = 0xFFu;
// Ease-out steepness. (1 - exp(-4)) normalization makes travel hit
// exactly 1.0 at t = 1.
const float EASE_K = 4.0;
const float EASE_NORM = 0.9816844; // 1.0 - exp(-EASE_K)

void unpack_w(
	float packed,
	out float scale,
	out float rotation,
	out float settle_time
) {
	uint bits = floatBitsToUint(packed);
	uint sscale = (bits >> 0u) & BYTE_MASK;
	uint rot = (bits >> 8u) & BYTE_MASK;
	uint settle = (bits >> 16u) & BYTE_MASK;

	// Map bytes back to the ranges packed in GoreParticleBuffer.cs
	scale = float(sscale) / 255.0 * 3.9 + 0.1; // 0.1 - 4.0
	rotation = float(rot) / 255.0 * TAU;
	settle_time = float(settle) / 255.0 * 1.73 + 0.02; // 0.02 - 1.75s
}

void vertex() {
	vec2 velocity = INSTANCE_CUSTOM.xy;
	float spawn_time = INSTANCE_CUSTOM.z;

	float scale;
	float rotation;
	float settle_time;
	unpack_w(INSTANCE_CUSTOM.w, scale, rotation, settle_time);

	float t = clamp((game_time - spawn_time) / settle_time, 0.0, 1.0);
	float travel = (1.0 - exp(-EASE_K * t)) / EASE_NORM;
	vec2 offset = velocity * settle_time * travel;

	float c = cos(rotation);
	float s = sin(rotation);
	vec2 corner = vec2(
		VERTEX.x * c - VERTEX.y * s,
		VERTEX.x * s + VERTEX.y * c
	);
	VERTEX = corner * scale + offset;
}
