shader_type canvas_item;

uniform vec4 colors[4] : source_color;
uniform int color_count = 0;

void fragment() {
	float pos = UV.y * float(color_count);
	int index = int(floor(pos));
	COLOR *= colors[index];
}