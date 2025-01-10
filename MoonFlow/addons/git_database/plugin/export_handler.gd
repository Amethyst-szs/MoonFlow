@tool
extends EditorExportPlugin
class_name EditorExportPluginGitDatabase

func _get_name() -> String:
	return "Export Plugin - Git Database"

func _export_begin(features: PackedStringArray, is_debug: bool, path: String, flags: int) -> void:
	# Generate source code
	var source: String = FileAccess.get_file_as_string("res://addons/git_database/template/template_gd.txt")
	
	var interface_inst := EditorPluginGitInterface.new()
	for method in interface_inst.get_method_list():
		if !method.name.begins_with("git_"):
			continue
		
		var result := interface_inst.call(method.name)
		
		if result is int:
			source = source.replace("{%s}" % method.name, str(result))
		elif result is String:
			source = source.replace("{%s}" % method.name, "\"%s\"" % result)
	
	# Create gdscript file
	var script := GDScript.new()
	script.source_code = source
	
	ResourceSaver.save(script, "res://addons/git_database/git.gd")
	
	# Cleanup
	interface_inst.free()