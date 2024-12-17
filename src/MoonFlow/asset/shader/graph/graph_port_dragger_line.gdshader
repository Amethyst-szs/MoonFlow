shader_type canvas_item;

uniform float dash_size = 0.05;
uniform float speed = 0.05f;
uniform float direction = 1.f;

void fragment() {
	float time = TIME * (speed * direction);
	float target = (SCREEN_UV.x * direction) + SCREEN_UV.y + time;
	
	float pos = mod(target, dash_size * 2.f);
	COLOR.a -= step(dash_size, pos);
}
