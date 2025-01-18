@tool
extends EditorExportPlugin
class_name EditorExportPluginWikiTableGenerator

func _get_name() -> String:
	return "Export Plugin - Wiki Table Generator"

func _export_begin(features: PackedStringArray, is_debug: bool, path: String, flags: int) -> void:
	var table := _generate_table()
	
	var out := JSON.new()
	out.parse(JSON.stringify(table))
	out.resource_path = "res://addons/wiki/plugin/table.json"
	
	ResourceSaver.save(out)

func _generate_table() -> Dictionary:
	return _generate_table_recursive("res://docs/")

func _generate_table_recursive(path: String) -> Dictionary:
	var files := DirAccess.get_files_at(path)
	var dirs := DirAccess.get_directories_at(path)
	
	var files_filtered: PackedStringArray = []
	for file in files:
		if file.ends_with(".md"): files_filtered.append(file)
	
	var value: Dictionary = {}
	
	value["__"] = files_filtered
	
	for dir in dirs:
		var res := _generate_table_recursive(path + dir + '/')
		if res["__"].size() == 0 && res.size() == 1: continue
		
		value[dir] = res
	
	return value
