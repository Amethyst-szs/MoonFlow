extends "res://scene/app/components/dialog_center_text.gd"

signal closed(is_accept: bool)

func _ready():
	super()
	
	canceled.connect(_on_close.bind(false))
	confirmed.connect(_on_close.bind(true))

func _on_close(is_accept: bool) -> void:
	closed.emit(is_accept)
