shader_type canvas_item;

uniform sampler2D star_texture;
uniform vec2 star_scale_range = vec2(0.1, 2.0);
uniform float density : hint_range(0.0, 1.0) = 0.2;
uniform vec4 background_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float colorful_chance : hint_range(0.0, 1.0, 0.01) = 0.1;

// Hash function for pseudo-random values
float hash(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

// 2D Random
vec2 rand2(vec2 p) {
	return vec2(hash(p), hash(p + 0.1234));
}

void fragment() {
	vec2 uv = FRAGCOORD.xy * min(SCREEN_PIXEL_SIZE.x, SCREEN_PIXEL_SIZE.y);

	// Convert UV to star grid coordinates
	vec2 cell_pos = floor(uv * 100.0);
	vec2 local_uv = fract(uv * 100.0);

	float star_chance = hash(cell_pos);
	float brightness = sin(FRAGCOORD.x * FRAGCOORD.y * 9.2);
	float red_mod = pow(sin(FRAGCOORD.x * FRAGCOORD.y * 3.5) * 0.5 + 0.5, colorful_chance);
	float green_mod = pow(sin(FRAGCOORD.x * FRAGCOORD.y * 1.2) * 0.5 + 0.5, colorful_chance);
	float blue_mod = pow(sin(FRAGCOORD.x * FRAGCOORD.y * 7.8) * 0.5 + 0.5, colorful_chance);

	vec4 color = background_color;

	if (star_chance < density) {
		// Use per-cell random for scale and offset
		vec2 rand = rand2(cell_pos);
		float scale = mix(star_scale_range.x, star_scale_range.y, rand.x);

		// Centered UV inside scaled star
		vec2 centered_uv = (local_uv - 0.5) / scale + 0.5;

		// Check if within star bounds
		if (all(greaterThanEqual(centered_uv, vec2(0.0))) && all(lessThanEqual(centered_uv, vec2(1.0)))) {
			vec4 star_col = texture(star_texture, centered_uv);
			star_col.r *= red_mod;
			star_col.g *= green_mod;
			star_col.b *= blue_mod;
			// Mix star over background
			color = mix(color, star_col, star_col.a * brightness);
		}
	}
	COLOR = color;
}