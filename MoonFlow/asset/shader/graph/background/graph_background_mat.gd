extends ShaderMaterial

var _theme_dict: Dictionary[String, Shader]
@export var theme_dict: Dictionary[String, Shader]:
	get:
		return _theme_dict
	set(value):
		_theme_dict = value
		shader = theme_dict[get_user_shader_choice()]

@export var translation_context: String

const key: String = "moonflow/event_graph/background_shader"

func get_user_shader_choice() -> String:
	var theme: String = EngineSettings.get_setting(key, "Default")
	if !theme_dict.has(theme):
		theme = "Default"
	
	return theme

func set_user_shader_choice(theme: String) -> void:
	if !theme_dict.has(theme):
		theme = "Default"
	
	EngineSettings.set_setting(key, theme)
	shader = theme_dict[theme]
