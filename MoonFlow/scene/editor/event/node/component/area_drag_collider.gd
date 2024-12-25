extends Area2D

@export var content: PanelContainer = null
@onready var shape: RectangleShape2D = null

var is_hover: bool = false

signal node_selected
signal node_multiselected
signal node_deselected

signal node_dragged(dist: Vector2)
signal node_drag_ended

func _ready() -> void:
	$Shape.shape = RectangleShape2D.new()
	shape = $Shape.shape
	
	_on_content_size_changed()

func _on_content_size_changed() -> void:
	if !is_instance_valid(shape):
		return
	
	position = content.size / 2
	shape.size = content.size

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		_unhandled_input_mouse_motion(event)
		return
	
	if !is_hover:
		return
	
	if event is InputEventMouseButton:
		_unhandled_input_mouse_button(event)
		return

func _unhandled_input_mouse_button(event: InputEventMouseButton) -> void:
	if event.button_index == MOUSE_BUTTON_LEFT:
		get_viewport().set_input_as_handled()
	else:
		return
	
	if !event.is_pressed():
		node_drag_ended.emit()
	
	if event.shift_pressed:
		node_multiselected.emit()
	else:
		node_selected.emit()

func _unhandled_input_mouse_motion(event: InputEventMouseMotion) -> void:
	if Input.get_mouse_button_mask() != MOUSE_BUTTON_MASK_LEFT:
		return
	
	node_dragged.emit(event.relative)
	get_viewport().set_input_as_handled()

func _on_selector_exited(area: Area2D) -> void:
	if area.monitorable:
		node_deselected.emit()

func _on_selector_entered(_area: Area2D) -> void: node_multiselected.emit()
func _on_mouse_entered(): is_hover = true
func _on_mouse_exited(): is_hover = false
