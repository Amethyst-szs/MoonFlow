@tool
extends EditorPlugin

var export_handler := EditorExportPluginWikiTableGenerator.new()

func _enter_tree() -> void:
	# Establish plugins
	add_export_plugin(export_handler)
	add_tool_menu_item("Local Wiki: Manually Generate Lookup Table", _update_table)
	
	# Ensure export presets are properly configured
	var exp_preset := ConfigFile.new()
	exp_preset.load("res://export_presets.cfg")
	
	var preset_id: int = 0
	while true:
		var section := "preset." + str(preset_id)
		if !exp_preset.has_section(section):
			break
		
		var data: String = exp_preset.get_value(section, "include_filter", "")
		if !data.contains("*.md"):
			printerr("\n\n----\nExport %s missing *.md include filter!\n-----\n\n" % section)
			break
		
		preset_id += 1

func _ready() -> void:
	_update_table()

func _build() -> bool:
	_update_table()
	return true

func _exit_tree() -> void:
	remove_export_plugin(export_handler)

func _update_table() -> void:
	export_handler._export_begin([], true, "", 0)
