extends Node

@export var zoom_factor: float = 0.1
@export var zoom_min: float = 0.2
@export var zoom_max: float = 3.0
@export var ui_bar_holder: EventFlowGraphScrollNavigationBars

var is_drag: bool = false

var zoom_pivot: Vector2 = Vector2.ZERO

@onready var parent: CanvasLayer = null

signal graph_offset_changed(offset: Vector2, scale: Vector2)

func _ready() -> void:
	parent = get_parent() as CanvasLayer

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion and is_drag:
		parent.offset += event.screen_relative / (parent.scale * 2.25)
		_clamp_offset_within_bounds()
		
		_update_position_for_scroll_navigation_ui()
		get_viewport().set_input_as_handled()
		return

func _unhandled_input(event: InputEvent) -> void:
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
	zoom_pivot = get_viewport().get_mouse_position()
	
	var factor := zoom_factor
	var new_scale := Vector2(parent.scale.x + factor, parent.scale.y + factor)
	_handle_zoom(new_scale)

func _handle_wheel_down() -> void:
	zoom_pivot = get_viewport().get_mouse_position()
	
	var factor := -zoom_factor
	var new_scale := Vector2(parent.scale.x + factor, parent.scale.y + factor)
	_handle_zoom(new_scale)

func _on_button_zoom_out_pressed():
	zoom_pivot = get_window().size / 2
	
	var factor := -zoom_factor * 2.5
	var new_scale := Vector2(parent.scale.x + factor, parent.scale.y + factor)
	_handle_zoom(new_scale)

func _on_button_zoom_in_pressed():
	zoom_pivot = get_window().size / 2
	
	var factor := zoom_factor * 2.5
	var new_scale := Vector2(parent.scale.x + factor, parent.scale.y + factor)
	_handle_zoom(new_scale)

func _on_button_zoom_reset_pressed():
	zoom_pivot = get_window().size / 2
	_handle_zoom(Vector2.ONE)

func _handle_zoom(new_scale: Vector2) -> void:
	# Return if new scale if out of bounds
	if new_scale.x < zoom_min || new_scale.x > zoom_max:
		return
	
	# Difference between current pivot and target
	var dif := parent.offset - zoom_pivot
	
	# Calculate relative scale
	var rel_scale := new_scale.x / parent.scale.x
	
	# Final offset after the scale transformation
	var final_offset := zoom_pivot + dif * rel_scale
	
	# Update transformation
	parent.scale = new_scale
	parent.offset = final_offset
	
	_clamp_offset_within_bounds()
	_update_position_for_scroll_navigation_ui()

func _clamp_offset_within_bounds() -> void:
	var fac := Vector2.ONE / ui_bar_holder.graph_scale
	var bound := ui_bar_holder.node_extent_rect
	bound.end *= fac
	
	var win_size := Vector2(get_window().size) * fac
	bound = bound.grow_side(SIDE_LEFT, win_size.x)
	bound = bound.grow_side(SIDE_TOP, win_size.y)
	
	parent.offset = -((-parent.offset).clamp(bound.position, bound.end))

func _update_position_for_scroll_navigation_ui() -> void:
	var factor := Vector2.ONE / parent.scale
	var pos := parent.offset * factor
	
	graph_offset_changed.emit(pos, factor)
