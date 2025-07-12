extends MarginContainer

var is_pushing: bool = false

@export var screen_size_margin: float = 87.0

@onready var parent_margin: HSplitContainer = get_parent()

func _ready() -> void:
	get_window().size_changed.connect(_on_window_size_changed)

func _process(_delta: float) -> void:
	if !is_pushing: return
	
	var win_y := get_window().size.y - screen_size_margin
	
	if size.y >= win_y:
		parent_margin.split_offset -= int(size.y - win_y)
	else:
		is_pushing = false

func _on_parent_split_dragged(_offset: int) -> void:
	is_pushing = true

func _on_window_size_changed() -> void:
	is_pushing = true
