extends HSlider

@export var key: String = ""

@export_group("Display")
@export var label: Label
@export var display_as_percent: bool = false
@export var percent_excludes_zero: bool = true
@export var display_factor: float = 1

@export var format: String = " %0.2f"
@export var suffix: String = ""

var _initial_label_text: String = ""

func _ready() -> void:
	_initial_label_text = label.text
	value = EngineSettings.get_setting(key, 1)
	
	value_changed.connect(_on_value_changed)
	_on_value_changed(value)

func _on_value_changed(v: float) -> void:
	EngineSettings.set_setting(key, v)
	
	if !display_as_percent:
		var display := v * display_factor
		label.text = tr(_initial_label_text) + (format % display) + suffix
	else:
		var dist = inverse_lerp(min_value, max_value, v) * 100
		
		if percent_excludes_zero:
			dist = clampf(dist, 2, 100)
		
		label.text = tr(_initial_label_text) + " %01.0f%%" % dist + suffix
