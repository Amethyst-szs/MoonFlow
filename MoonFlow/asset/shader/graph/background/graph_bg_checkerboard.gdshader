shader_type canvas_item;

uniform vec2 graph_position;
uniform vec2 dot_size;
uniform float parallax = 0.9f;

void fragment() {
	vec2 pos = vec2(FRAGCOORD.xy) + (graph_position * parallax);
	vec2 tile = mod(pos, dot_size * 2.f);
	
	if (tile.x > dot_size.x != tile.y > dot_size.y)
		COLOR.rgb += vec3(0.03);
}
