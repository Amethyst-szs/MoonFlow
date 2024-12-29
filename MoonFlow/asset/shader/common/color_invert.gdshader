shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

void fragment() {
	vec3 col = textureLod(screen_texture, SCREEN_UV, 0.0).rgb;
	float m = max(max(col.r, col.g), col.b);
	COLOR.rgb = vec3(1.0f) - vec3(m);
}