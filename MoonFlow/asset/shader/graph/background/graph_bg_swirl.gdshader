shader_type canvas_item;

uniform vec2 graph_position;
uniform vec2 dot_size;
uniform float parallax = 0.9f;

float circle(vec2 position, float radius, float feather)
{
	return smoothstep(radius, radius + feather, length(position - vec2(0.5)));
}

float swirl(vec2 uv, float size, int arms)
{
	float angle = atan(-uv.y + 0.5, uv.x - 0.5) ;
	float len = length(uv - vec2(0.5, 0.5));
	
	return sin(len * size + angle * float(arms));
}

void fragment()
{
	vec2 pos = (FRAGCOORD.xy * SCREEN_PIXEL_SIZE) + vec2(0.5f) + (graph_position * SCREEN_PIXEL_SIZE);
	float swirl = swirl(pos, (1.0 / dot_size.x) * 800.0, 24);
	float swirl_step = smoothstep(0.5, 1.0, swirl);
	
	if (swirl_step != 0.f || circle(pos, 0.005f, 0.0f) == 0.f)
		COLOR.rgb += vec3(0.03);
}
