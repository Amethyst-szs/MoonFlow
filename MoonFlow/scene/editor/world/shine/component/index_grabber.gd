extends HBoxContainer

@export var holder: PanelContainer

var is_drag: bool = false:
	get: return is_drag
	set (value):
		is_drag = value
		
		if value:
			modulate = Color.WHITE
		else:
			modulate = Color.GRAY

signal index_changed(idx: int)

func _ready() -> void:
	is_drag = false

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton && !event.is_pressed():
		var m := event as InputEventMouseButton
		
		if m.button_index == MOUSE_BUTTON_LEFT:
			is_drag = false
	
	if event is InputEventMouseMotion and is_drag:
		_try_change_index_by_drag()

func _gui_input(event: InputEvent) -> void:
	if event is not InputEventMouseButton:
		return
	
	var m := event as InputEventMouseButton
	if m.button_index != MOUSE_BUTTON_LEFT || !m.is_pressed():
		return
	
	is_drag = true

func _try_change_index_by_drag() -> void:
	if !is_drag:
		return
	
	var margin := holder.size.y / 2.0
	var origin := holder.get_local_mouse_position().y - (margin)
	
	var index := holder.get_index()
	var offset := 0
	
	if origin > margin:
		offset = 1
	else: if origin < -margin:
		offset = -1
	if offset == 0:
		return
	
	var old_index: int = index
	index = clampi(index + offset, 0, holder.get_parent().get_child_count() - 1)
	
	if index != old_index:
		index_changed.emit(index)
