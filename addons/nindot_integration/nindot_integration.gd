@tool
extends EditorPlugin

var plugin_sarc: SarcResourceImport = null

func _enter_tree():
	if !Engine.is_editor_hint():
		return
	
	# Setup all import plugins
	plugin_sarc = SarcResourceImport.new()
	add_import_plugin(plugin_sarc)

func _exit_tree():
	if !Engine.is_editor_hint():
		return
	
	# Remove all import plugins
	remove_import_plugin(plugin_sarc)
	plugin_sarc = null
