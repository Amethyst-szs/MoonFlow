@tool
extends VBoxContainer

@export var input_action: String
@export var is_debug_only: bool = false

const trash_icon: Texture2D = preload("res://asset/material/file/trash.svg")
const capture_scene: PackedScene = preload("res://scene/settings/engine/keybind/overlay_keybind_capture.tscn")

@onready var label_name: Label = $Panel/HBox_Layout/Label_Name
@onready var bind_table: VBoxContainer = $Panel/HBox_Layout/VBox_BindTable

func _ready():
	if Engine.is_editor_hint():
		label_name.text = tr(input_action, "INPUT_MAP")
		return
	
	_setup_content()
	
	if !OS.is_debug_build() && is_debug_only:
		queue_free()

func _setup_content() -> void:
	# Clear bind table
	for child in bind_table.get_children():
		child.queue_free()
	
	# Setup header
	label_name.text = tr(input_action, "INPUT_MAP")
	
	# Create bind table
	for bind in InputMap.action_get_events(input_action):
		var c := _on_trash_pressed.bind(bind)
		_create_button(bind.as_text(), trash_icon, c)

func _create_button(text: String, ico: Texture2D, c: Callable) -> void:
	var button := Button.new()
	button.pressed.connect(c.bind(button))
	bind_table.add_child(button)
	
	button.text = text
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	button.icon = ico
	button.icon_alignment = HORIZONTAL_ALIGNMENT_LEFT

func _on_trash_pressed(button: Button, event: InputEvent) -> void:
	InputMap.action_erase_event(input_action, event)
	
	var output: Dictionary = {
		"deadzone": 0.5,
		"events": InputMap.action_get_events(input_action),
	}
	
	EngineSettings.set_setting("input/" + input_action, output)
	button.queue_free()

func _on_add_pressed() -> void:
	var scene := capture_scene.instantiate()
	get_tree().current_scene.add_child(scene)
	
	scene.key_accepted.connect(_on_add_key_accepted)

func _on_add_key_accepted(event: InputEventKey) -> void:
	InputMap.action_add_event(input_action, event)
	
	var output: Dictionary = {
		"deadzone": 0.5,
		"events": InputMap.action_get_events(input_action),
	}
	
	EngineSettings.set_setting("input/" + input_action, output)
	
	var c := _on_trash_pressed.bind(event)
	_create_button(event.as_text(), trash_icon, c)

func _get_configuration_warnings() -> PackedStringArray:
	label_name.text = tr(input_action, "INPUT_MAP")
	return []
