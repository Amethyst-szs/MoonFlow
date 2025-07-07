@tool
extends Label

var _label_text_internal: String = ""

@export_multiline var label_text: String = "":
	get:
		return _label_text_internal
	set(value):
		_label_text_internal = value
		
		var idx: int = 0
		var utf8: PackedByteArray = value.to_utf8_buffer()
		
		while idx < value.length():
			if utf8[idx] >= 0x41 && utf8[idx] <= 0x5A:
				value = value.insert(idx, '​') # Zero width space character
				utf8.insert(idx, 0x0) # Null byte
				
				idx += 1
			
			idx += 1
		
		text = value
		
		await Engine.get_main_loop().process_frame
		_calculate_font_size()

func _ready() -> void:
	label_settings = LabelSettings.new()
	
	get_window().size_changed.connect(_on_window_size_changed)
	
	await Engine.get_main_loop().process_frame
	_calculate_font_size()

func _on_visibility_changed() -> void:
	await Engine.get_main_loop().process_frame
	_calculate_font_size()

func _on_window_size_changed() -> void:
	_calculate_font_size()

#region Utility

func _calculate_font_size() -> void:
	label_settings.font_size = 18
	
	while get_line_count() > get_visible_line_count():
		label_settings.font_size -= 1
		
		if label_settings.font_size <= 11:
			break

#endregion
