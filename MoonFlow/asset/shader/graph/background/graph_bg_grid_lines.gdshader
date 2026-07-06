shader_type canvas_item;

uniform vec2 graph_position;
uniform vec2 dot_size;
uniform float parallax = 0.9f;

void fragment() {
	vec2 pos = vec2(FRAGCOORD.xy) + (graph_position * parallax);
	
	vec2 tile = mod(pos, dot_size);
	vec2 contained = step(dot_size / 10.f, tile);
	
	if (contained.x == 0.f || contained.y == 0.f)
		COLOR.rgb += vec3(0.025);
}
