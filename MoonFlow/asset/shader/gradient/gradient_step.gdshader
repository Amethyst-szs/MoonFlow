shader_type canvas_item;

uniform vec4 first_color : source_color = vec4(1.0);
uniform vec4 second_color : source_color = vec4(1.0);
uniform float position : hint_range(-2, 2) = 0.5;
uniform float color_angle = 0.0;
uniform int stepIntensify : hint_range(1, 50) = 10;

vec2 rotate(vec2 uv, vec2 pivot, float angle)
{
	mat2 rotation = mat2(vec2(sin(angle), -cos(angle)),
						vec2(cos(angle), sin(angle)));

	uv -= pivot;
	uv = uv * rotation;
	uv += pivot;
	return uv;
}

void fragment() {
	vec2 uv = rotate(UV, vec2(0.5, 0.5), color_angle);

	float pos = round(smoothstep(0,1,(position + uv.y) / 2.0) * float(stepIntensify));
	COLOR = mix(first_color,second_color,pos/float(stepIntensify));
}