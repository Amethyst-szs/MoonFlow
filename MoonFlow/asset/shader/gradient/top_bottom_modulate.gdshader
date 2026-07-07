shader_type canvas_item;

uniform vec4 first_color : source_color = vec4(1.0);
uniform vec4 second_color : source_color = vec4(1.0);

void fragment() {
	vec3 factor = mix(first_color, second_color, UV.y).rgb;
	float mask = min(1.0f - length(COLOR.rg), length(COLOR.b));
	if (mask > 0.9f)
		COLOR.rgb = factor;
}
