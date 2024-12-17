extends Area2D

@export var shape: CollisionShape2D

var is_active: bool = false
var is_hold_shift: bool = false
var initial_position: Vector2 = Vector2.ZERO

signal selection_started
signal selection_started_with_shift
signal selection_ended

const style: StyleBoxFlat = preload(
	"res://asset/theme/event/graph/stylebox/graph_mouse_selection.tres"
)

func _ready() -> void:
	visibility_changed.connect(_on_visiblity_changed)
	hide()

func _on_visiblity_changed() -> void:
	monitorable = visible
	is_active = visible
	
	position = Vector2.ZERO
	(shape.shape as RectangleShape2D).size = Vector2.ZERO
	
	if is_active:
		if is_hold_shift:
			selection_started_with_shift.emit()
		else:
			selection_started.emit()
	else:
		selection_ended.emit()
	
	initial_position = get_global_mouse_position()

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		is_hold_shift = event.shift_pressed
		
		if event.button_index == MOUSE_BUTTON_LEFT:
			visible = event.pressed
			get_viewport().set_input_as_handled()

func _process(_delta: float) -> void:
	if !is_active:
		return
	
	var mouse_pos := get_global_mouse_position()
	shape.global_position = initial_position - ((initial_position - mouse_pos) / 2)
	(shape.shape as RectangleShape2D).size = abs(mouse_pos - initial_position)
	
	queue_redraw()

func _draw() -> void:
	var rect := shape.shape.get_rect()
	rect.position += shape.global_position
	draw_style_box(style, rect)
