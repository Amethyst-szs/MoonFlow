@tool
class_name RichTextPreviewShakeTag
extends RichTextEffect

var bbcode = "shaketag"

func _process_custom_fx(char_fx: CharFXTransform):
	var time: float = char_fx.elapsed_time + (float(char_fx.relative_index) / 40)
	var angle: float = (sin(time * 14) / 3) - (PI / 9)
	
	var matrix = char_fx.transform
	
	matrix = matrix.translated_local(Vector2(angle * -5, angle * -10))
	matrix = matrix.rotated_local(angle)
	
	char_fx.transform = matrix
	return true
