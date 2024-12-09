@tool
class_name RichTextPreviewTremble
extends RichTextEffect

var bbcode = "tremble"

func _process_custom_fx(char_fx: CharFXTransform):
	char_fx.transform = char_fx.transform.rotated_local(randf_range(-0.09, 0.09))
	return true
