@tool
class_name RichTextPreviewScream
extends RichTextEffect

var bbcode = "scream"

func _process_custom_fx(char_fx: CharFXTransform):
	var time = (char_fx.elapsed_time * 100) + (float(char_fx.relative_index) / 5)
	var dist = sin(time) * 4
	
	char_fx.transform = char_fx.transform.translated_local(Vector2(0, dist))
	return true
