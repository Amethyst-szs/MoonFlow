@tool
extends EditorPlugin

var parser_plugin: EditorTranslationParserPlugin

const input_map_target: String = "res://addons/podb/input_map.tres"

func _enter_tree():
	# Setuo podb parser
	parser_plugin = load("res://addons/podb/parser.gd").new()
	add_translation_parser_plugin(parser_plugin)
	
	# Setup input map translation DB
	add_tool_menu_item("poDB: Regenerate Input Map Translation Bank", 
		_setup_input_map_translation_bank)

func _setup_input_map_translation_bank() -> void:
	if !Engine.is_editor_hint():
		return
	
	var input_map_bank := TranslationBank.new()
	input_map_bank.context = "INPUT_MAP"
	
	var editor_actions := InputMap.get_actions()
	for action in _project_settings_access_util("input/"):
		var n = action.trim_prefix("input/")
		if !editor_actions.has(n) && !n.ends_with(".macos"):
			input_map_bank.keys.append(n)
	
	ResourceSaver.save(input_map_bank, input_map_target)

func _project_settings_access_util(filter: String) -> PackedStringArray:
	var db := ProjectSettings.get_property_list().map(func(prop): return prop.name).filter(func(prop): return ProjectSettings.has_setting(prop))
	db = db.filter(_project_settings_access_util_filter.bind(filter))
	return PackedStringArray(db)

func _project_settings_access_util_filter(str: String, filter: String) -> bool:
	return str.begins_with(filter)

func _exit_tree():
	remove_translation_parser_plugin(parser_plugin)
