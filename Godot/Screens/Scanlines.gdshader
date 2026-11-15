shader_type canvas_item;

uniform float intensity : hint_range(0.0, 0.5) = 0.0;

void fragment() {
    float band = step(0.5, fract(UV.y / TEXTURE_PIXEL_SIZE.y));
    COLOR.rgb *= 1.0 - intensity * band;
}
