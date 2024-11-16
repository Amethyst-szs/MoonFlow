@tool
extends EditorPlugin

const romfs_100_path_key: String = "application/nindot/romfs_100_path_for_debug"
const romfs_130_path_key: String = "application/nindot/romfs_130_path_for_debug"

var plugin_sarc: SarcResourceImport = null

func _enter_tree():
	if !Engine.is_editor_hint():
		return
	
	# Define additional project settings
	if !ProjectSettings.has_setting(romfs_100_path_key):
		ProjectSettings.set_setting(romfs_100_path_key, "")
		ProjectSettings.save()
	
	if !ProjectSettings.has_setting(romfs_130_path_key):
		ProjectSettings.set_setting(romfs_130_path_key, "")
		ProjectSettings.save()
	
	# Setup all import plugins
	plugin_sarc = SarcResourceImport.new()
	add_import_plugin(plugin_sarc)

func _exit_tree():
	if !Engine.is_editor_hint():
		return
	
	# Remove all import plugins
	remove_import_plugin(plugin_sarc)
	plugin_sarc = null