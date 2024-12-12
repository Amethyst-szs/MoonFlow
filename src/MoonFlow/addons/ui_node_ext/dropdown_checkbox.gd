class_name DropdownCheckbox
extends CheckBox

var _dropdown: Control = null

@export var dropdown: Control:
	get: 
		return _dropdown
	set(value):
		_dropdown = value
		_on_toggled(button_pressed)

func _ready() -> void:
	toggled.connect(_on_toggled)
	_on_toggled(button_pressed)

func _on_toggled(state: bool) -> void:
	if dropdown:
		dropdown.visible = state
