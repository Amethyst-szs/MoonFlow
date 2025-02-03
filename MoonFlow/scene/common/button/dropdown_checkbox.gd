class_name DropdownCheckbox
extends CheckBox

var _dropdown: Control = null
@export var dropdown: Control:
	get: 
		return _dropdown
	set(value):
		if _dropdown != null:
			_dropdown.remove_meta("dropdown")
		
		_dropdown = value
		value.set_meta("dropdown", self)
		_on_toggled(button_pressed)

@export var invert_result: bool = false

func _enter_tree() -> void:
	set_meta("dropdown_button", true)

func _ready() -> void:
	toggled.connect(_on_toggled)
	visibility_changed.connect(_on_visibility_changed)
	_on_toggled(button_pressed)

func _on_toggled(state: bool) -> void:
	if invert_result:
		state = !state
	
	if dropdown:
		dropdown.visible = state

func _on_visibility_changed() -> void:
	var result := button_pressed && visible
	if invert_result:
		result = !result
	
	if dropdown:
		dropdown.visible = result
