extends Node

func _ready() -> void:
	set_minimum_size()

static func reset_display_server() -> void:
	# Reset display server mode
	DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)
	
	# Reset window position and size
	var win: Window = Engine.get_main_loop().current_scene.get_window()
	
	const x: int = 1152
	const y: int = 648
	win.size = Vector2i(x, y)
	
	win.move_to_center()
	win.grab_focus()
	
	set_minimum_size()

static func set_minimum_size() -> void:
	DisplayServer.window_set_min_size(Vector2i(799, 527))
