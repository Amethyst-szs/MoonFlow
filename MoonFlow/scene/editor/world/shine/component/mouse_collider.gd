extends Node2D

@onready var parent: PanelContainer = get_parent()

var state: bool = false

signal mouse_entered
signal mouse_exited

func _process(_delta: float) -> void:
	var m: Vector2 = get_global_mouse_position()
	var is_hover: bool = parent.get_global_rect().has_point(m)
	
	if is_hover && !state:
		mouse_entered.emit.call_deferred()
		state = true
	else: if !is_hover && state:
		mouse_exited.emit()
		state = false

func _on_world_shine_editor_holder_visibility_changed() -> void:
	if !is_instance_valid(parent):
		return
	
	if !parent.is_visible_in_tree(): process_mode = Node.PROCESS_MODE_DISABLED
	else: process_mode = Node.PROCESS_MODE_INHERIT
