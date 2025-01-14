extends PanelContainer

var is_grabbed: bool = false

const grab_offset := Vector2(12, 12)

@onready var parent: PanelContainer = $"../../"

func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		_gui_input_mouse_button(event)
		return
	
	if event is InputEventMouseMotion:
		_gui_input_mouse_moition(event)
		return

func _gui_input_mouse_button(event: InputEventMouseButton) -> void:
	if event.button_index != MOUSE_BUTTON_LEFT:
		return
	
	get_viewport().set_input_as_handled()
	is_grabbed = event.pressed

func _gui_input_mouse_moition(event: InputEventMouseMotion) -> void:
	if !is_grabbed: return
	get_viewport().set_input_as_handled()
	
	var canvas := get_canvas_layer_node()
	if !canvas: return
	
	var mpos := (event.global_position - canvas.offset) * (Vector2.ONE / canvas.scale)
	parent.size = (mpos - parent.global_position) + grab_offset
