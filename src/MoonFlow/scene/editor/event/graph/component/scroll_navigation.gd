extends Node

@export var zoom_factor: float = 0.1
@export var zoom_min: float = 0.2
@export var zoom_max: float = 3.0

var is_drag: bool = false

@onready var parent: CanvasLayer = null

func _ready() -> void:
	parent = get_parent() as CanvasLayer

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and is_drag:
		parent.offset += event.screen_relative / (parent.scale * 2.25)
	
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_MIDDLE:
			_handle_drag_toggle(event.pressed)
			return
		
		if event.button_index == MOUSE_BUTTON_WHEEL_UP and event.pressed:
			_handle_wheel_up()
		
		if event.button_index == MOUSE_BUTTON_WHEEL_DOWN and event.pressed:
			_handle_wheel_down()
	

func _handle_drag_toggle(is_pressed: bool) -> void:
	is_drag = is_pressed
	
	if is_drag:
		DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_CAPTURED)
	else:
		DisplayServer.mouse_set_mode(DisplayServer.MOUSE_MODE_VISIBLE)
	
	get_viewport().set_input_as_handled()

func _handle_wheel_up() -> void:
	_handle_wheel(1)

func _handle_wheel_down() -> void:
	_handle_wheel(-1)

func _handle_wheel(direction: float) -> void:
	# Calculate target scale
	var factor := direction * zoom_factor
	var new_scale := Vector2(parent.scale.x + factor, parent.scale.y + factor)
	
	# Return if new scale if out of bounds
	if new_scale.x < zoom_min || new_scale.x > zoom_max:
		return
	
	# Difference between current pivot and target
	var pivot := get_viewport().get_mouse_position()
	var dif := parent.offset - pivot
	
	# Calculate relative scale
	var rel_scale := new_scale.x / parent.scale.x
	
	# Final offset after the scale transformation
	var final_offset := pivot + dif * rel_scale
	
	# Update transformation
	parent.scale = new_scale
	parent.offset = final_offset
