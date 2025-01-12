extends "res://scene/common/line_edit/filter_input_to_regex.gd"

func emit_text() -> void:
	await Engine.get_main_loop().process_frame
	text_validated.emit(text)

func _unhandled_key_input(event: InputEvent) -> void:
	if !is_visible_in_tree():
		return
	
	if event.is_action_pressed("ui_find", false, true):
		grab_focus()
		grab_click_focus()
		
		get_viewport().set_input_as_handled()
