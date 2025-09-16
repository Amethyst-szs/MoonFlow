extends Control

@onready var button: Button = $Button
@onready var circle: Panel = $Panel_Circle

signal pressed

func init_button(msg: String) -> void:
	await Engine.get_main_loop().process_frame
	
	button.text = msg

func assign_neighbors(left: Control, right: Control) -> void:
	await Engine.get_main_loop().process_frame
	
	var leftP = left.get_node("Button").get_path()
	var rightP = right.get_node("Button").get_path()
	
	button.focus_neighbor_left = leftP
	button.focus_neighbor_right = rightP
	
	button.focus_previous = leftP
	button.focus_next = rightP

func _on_button_highlight_update() -> void:
	await Engine.get_main_loop().process_frame
	
	var is_highlight: bool = button.is_hovered() || button.has_focus()
	
	if is_highlight:
		circle.modulate = Color.RED
	else:
		circle.modulate = Color.WHITE

func _on_button_pressed():
	pressed.emit()
