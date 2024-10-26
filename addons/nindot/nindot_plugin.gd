@tool
extends EditorPlugin

# This file is intentionally separate from the nuget package of Nindot.
# In order to get a full installation of Nindot into another Godot project,
# install the C# nuget package as well as copying over this addons folder.
# If the addon folder isn't copied over, the editor will not support
# resource importing of Nindot file formats (SZS, SARC, BYML, MSBT, ect.)

var plugin_sarc: SarcResourceImport = null
var plugin_byml: BymlResourceImport = null
var plugin_msbt: MsbtResourceImport = null

func _enter_tree():
	if !Engine.is_editor_hint():
		return
	
	plugin_sarc = SarcResourceImport.new()
	add_import_plugin(plugin_sarc)
	
	plugin_byml = BymlResourceImport.new()
	add_import_plugin(plugin_byml)
	
	plugin_msbt = MsbtResourceImport.new()
	add_import_plugin(plugin_msbt)

func _exit_tree():
	if !Engine.is_editor_hint():
		return
	
	remove_import_plugin(plugin_sarc)
	plugin_sarc = null
	
	remove_import_plugin(plugin_byml)
	plugin_byml = null
	
	remove_import_plugin(plugin_msbt)
	plugin_msbt = null
