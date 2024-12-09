@tool
class_name RichTextPreviewBeat
extends RichTextEffect

var bbcode = "beat"

func _process_custom_fx(char_fx: CharFXTransform):
	var time: float = fmod(char_fx.elapsed_time, 1.5)
	var scale: float = maxf(1.1 - pow(fmod(time, 1.5), 1.8), 1)
	
	char_fx.transform = char_fx.transform.scaled_local(Vector2(scale, scale))
	return true
