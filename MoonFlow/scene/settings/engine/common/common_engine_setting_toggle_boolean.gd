extends CheckBox

@export var key: String = ""
@export var default_value: bool = false

func _ready() -> void:
	var value = EngineSettings.get_setting(key, default_value)
	set_pressed_no_signal(value)

func _pressed() -> void:
	EngineSettings.set_setting(key, button_pressed)
