shader_type canvas_item;

uniform vec4 first_color : source_color = vec4(1.0f);
uniform vec4 second_color : source_color = vec4(1.0f);
uniform vec2 rect_global_pos = vec2(0.0f);
uniform vec2 rect_size = vec2(0.0f);

void fragment() {
	vec2 pos = (SCREEN_UV - rect_global_pos) / rect_size;
	vec3 factor = mix(first_color, second_color, pos.y).rgb;
	float mask = min(1.0f - length(COLOR.rg), length(COLOR.b));
	if (mask > 0.9f)
		COLOR.rgb = factor;
}
