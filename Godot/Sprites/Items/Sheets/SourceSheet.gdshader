shader_type canvas_item;

void fragment() {
	// The supplied sheets use opaque magenta around their original pixel art.
	if (all(equal(texture(TEXTURE, UV).rgb, vec3(1.0, 0.0, 1.0)))) {
		COLOR.a = 0.0;
	}
}
