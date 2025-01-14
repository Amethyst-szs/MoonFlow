extends Label

@onready var parent: PanelContainer = $"../../../../"

var is_grabbed: bool = false
var grab_offset := Vector2.ZERO

signal node_moved()
signal node_edit_page_opened()

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
	
	if event.is_command_or_control_pressed():
		node_edit_page_opened.emit()
		return
	
	is_grabbed = event.pressed
	grab_offset = parent.get_local_mouse_position()

func _gui_input_mouse_moition(event: InputEventMouseMotion) -> void:
	if !is_grabbed: return
	get_viewport().set_input_as_handled()
	
	var canvas := get_canvas_layer_node()
	if !canvas: return
	
	var mpos := (event.global_position - canvas.offset) * (Vector2.ONE / canvas.scale)
	parent.global_position = mpos - grab_offset
	node_moved.emit()
