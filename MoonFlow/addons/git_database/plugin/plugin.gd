@tool
extends EditorPlugin

var export_handler := EditorExportPluginGitDatabase.new()
var info_inst := GitInfo.new()

var tools: Dictionary = {
	"Git Database: Update DB": _manual_update,
	"Git Database: Log Repo Info": EditorPluginGitInterface.log_all_info,
}

func _enter_tree():
	# Setup export plugin
	add_autoload_singleton("GitInfoSingletonRegisterUtility", "res://addons/git_database/plugin/instance_register.gd")
	add_export_plugin(export_handler)
	
	# Setup tool menu
	for i in range(tools.size()):
		add_tool_menu_item(tools.keys()[i], tools.values()[i])

func _exit_tree():
	# Cleanup export plugin
	remove_autoload_singleton("GitInfoSingletonRegisterUtility")
	remove_export_plugin(export_handler)
	
	# Cleanup tool menu
	for i in range(tools.size()):
		remove_tool_menu_item(tools.keys()[i])

func _manual_update() -> void:
	export_handler._export_begin([], true, "", 0)
