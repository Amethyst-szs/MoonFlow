@tool
extends EditorPlugin

var export_handler := EditorExportPluginGitDatabase.new()

var tools: Dictionary = {
	"Git Database: Update DB": _manual_update_forced,
	"Git Database: Log Repo Info": EditorPluginGitInterface.log_all_info,
}

func _enter_tree():
	# Setup export plugin
	add_export_plugin(export_handler)
	
	# If the git DB file doesn't already exist locally, create it
	if !FileAccess.file_exists("res://addons/git_database/git.gd"):
		_manual_update()
	
	# Setup tool menu
	for i in range(tools.size()):
		add_tool_menu_item(tools.keys()[i], tools.values()[i])

func _exit_tree():
	# Cleanup export plugin
	remove_export_plugin(export_handler)
	
	# Cleanup tool menu
	for i in range(tools.size()):
		remove_tool_menu_item(tools.keys()[i])

func _build() -> bool:
	_manual_update()
	return true

func _manual_update() -> void:
	export_handler._export_begin([], true, "", 0)

func _manual_update_forced() -> void:
	export_handler._export_begin(["force"], true, "", 0)
