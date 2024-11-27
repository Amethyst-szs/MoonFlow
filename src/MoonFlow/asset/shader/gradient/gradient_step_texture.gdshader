shader_type canvas_item;

uniform sampler2D tex;

uniform vec4 first_color : source_color = vec4(1.0);
uniform vec4 second_color : source_color = vec4(1.0);
uniform float position : hint_range(-2, 2) = 0.5;
uniform int stepIntensify : hint_range(1, 50) = 10;

void fragment() {
	float pos = round(smoothstep(0,1,(position + UV.y) / 2.0) * float(stepIntensify));
	COLOR = min(mix(first_color,second_color,pos/float(stepIntensify)), texture(tex, UV));
}