shader_type canvas_item;

uniform int sheet_position : hint_range(0, 300, 0.1) = 0;
uniform int sheet_width : hint_range(1, 50, 0.1) = 1;
uniform int sheet_height : hint_range(1, 50, 0.1) = 1;

void vertex() {
	UV.x /= float(sheet_width);
	UV.y /= float(sheet_height);
	
	UV.x += float(sheet_position % sheet_width) / float(sheet_width);
	UV.y += float(sheet_position / sheet_width) / float(sheet_height);
}
