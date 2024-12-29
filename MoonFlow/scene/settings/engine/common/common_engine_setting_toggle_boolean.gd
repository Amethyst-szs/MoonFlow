extends CheckBox

@export var key: String = ""

func _ready() -> void:
	var value = EngineSettings.get_setting(key, false)
	set_pressed_no_signal(value)

func _pressed() -> void:
	EngineSettings.set_setting(key, button_pressed)
