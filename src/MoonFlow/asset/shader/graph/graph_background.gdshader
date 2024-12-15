shader_type canvas_item;

uniform vec2 graph_position;

uniform vec2 dot_size;

vec2 rotate(vec2 uv, vec2 pivot, float angle)
{
	mat2 rotation = mat2(vec2(sin(angle), -cos(angle)), vec2(cos(angle), sin(angle)));
	uv -= pivot;
	uv = uv * rotation;
	uv += pivot;
	return uv;
}

void fragment() {
	vec2 pos = vec2(FRAGCOORD.xy) + graph_position;
	pos = rotate(pos, vec2(0), radians(45));

	vec2 tile = mod(pos, dot_size);
	vec2 contained = step(dot_size / 2.f, tile);

	if (contained.x != 0.f && contained.y != 0.f)
		COLOR.rgb += vec3(0.03);

	vec2 tile_large = mod(pos, dot_size * 2.f);
	vec2 contained_large = step(dot_size, tile_large);

	if (contained_large.x != 0.f && contained_large.y != 0.f)
		COLOR.rgb += vec3(0.02);

	//if (tile.x > dot_size.x / 2.f && tile.y > dot_size.y / 2.f)
		//COLOR.rgb += vec3(0.1f);
}
