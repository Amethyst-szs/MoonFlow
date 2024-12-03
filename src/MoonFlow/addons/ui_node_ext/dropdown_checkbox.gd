class_name DropdownCheckbox
extends CheckBox

@export var dropdown: Control

func _ready() -> void:
	toggled.connect(_on_toggled)
	_on_toggled(button_pressed)

func _on_toggled(state: bool) -> void:
	if dropdown:
		dropdown.visible = state
