extends PanelContainer

signal key_accepted(event: InputEventKey)

const key_blacklist: PackedInt32Array = [
	KEY_ESCAPE,
	KEY_TAB,
	KEY_BACKTAB,
	KEY_BACKSPACE,
	KEY_ENTER,
	KEY_KP_ENTER,
	
	KEY_SHIFT,
	KEY_CTRL,
	KEY_META,
	KEY_ALT,
	KEY_CAPSLOCK,
	KEY_SCROLLLOCK,
]

func _input(event: InputEvent) -> void:
	get_viewport().set_input_as_handled()
	if !event.is_pressed(): return
	
	if event is InputEventKey:
		if event.keycode == KEY_ESCAPE:
			queue_free()
			return
		
		if key_blacklist.has(event.keycode):
			return
		
		key_accepted.emit(event as InputEventKey)
		queue_free()
