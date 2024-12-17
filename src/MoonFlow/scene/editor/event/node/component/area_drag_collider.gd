extends Area2D

@export var content: PanelContainer = null
@onready var shape: RectangleShape2D = $Shape.shape as RectangleShape2D

var is_hover: bool = false
var is_select: bool = false

func _ready() -> void:
	_on_content_size_changed()

func _on_content_size_changed() -> void:
	if !is_instance_valid(shape):
		return
	
	position = content.size / 2
	shape.size = content.size

func _unhandled_input(event: InputEvent) -> void:
	if event is not InputEventMouseButton or !is_hover:
		return
	
	var m = event as InputEventMouseButton
	if m.button_index == MOUSE_BUTTON_LEFT && m.pressed:
		is_select = true

func _on_selector_entered(_area: Area2D) -> void: is_select = true
func _on_selector_exited(_area: Area2D) -> void: is_select = false
func _on_mouse_entered(): is_hover = true
func _on_mouse_exited(): is_hover = false
