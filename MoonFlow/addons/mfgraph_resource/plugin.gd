@tool
extends EditorPlugin

var import_plugin: MfgraphImportPlugin = null

func _enter_tree():
	import_plugin = MfgraphImportPlugin.new()
	add_import_plugin(import_plugin)

func _exit_tree():
	remove_import_plugin(import_plugin)
	import_plugin = null
