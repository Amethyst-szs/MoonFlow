@tool
extends EditorPlugin

## Edit "addons/AutoExportVersion" project settings to configure the plugin
## and create version config file.

####################################################################################################


## Locations where the version can be stored See [member STORE_LOCATION].
enum VersionStoreLocation {
	## Store the version in script at path from [member SCRIPT_PATH].
	SCRIPT, 
	## Store the version in project setting [member PROJECT_SETTING_NAME].
	PROJECT_SETTING,
}

## Determines where the version is saved when exporting. See [member VersionStoreLocation].                                       [br]
var STORE_LOCATION: VersionStoreLocation = VersionStoreLocation.PROJECT_SETTING

## Path to the version script file where it is going to be saved. See [member SCRIPT_TEMPLATE]
var SCRIPT_PATH: String = "res://version.gd"
## This template String is going to be formatted so that it contains the version.
const SCRIPT_TEMPLATE: String ="extends RefCounted\nconst VERSION: String = \"{version}\""
## Name of the project setting where the version is going to be stored as a String.
var PROJECT_SETTING_NAME: String = "application/config/version"
## Path to the configuration file for the plugin.
var CONFIG_PATH = "res://auto_export_version_config_file.gd"


## Stores a [param version] based on [param version_store_location].                            [br]
## See [member PROJECT_SETTING_NAME], [member SCRIPT_PATH]
func store_version(version: String, version_store_location := VersionStoreLocation.PROJECT_SETTING) -> void:
	match version_store_location:
		VersionStoreLocation.SCRIPT:
			store_version_as_script(version)
		VersionStoreLocation.PROJECT_SETTING:
			store_version_as_project_setting(version)

## Stores the version as a script based on [member SCRIPT_TEMPLATE] in [member SCRIPT_PATH].
func store_version_as_script(version: String) -> void:
	if version.is_empty():
		printerr("Cannot store version. " + _EMPTY_VERSION_ERROR.format({"script_path": get_script().get_path()}))
		return
	
	var script: GDScript = GDScript.new()
	script.source_code = SCRIPT_TEMPLATE.format({"version": version})
	var err: int = ResourceSaver.save(script, SCRIPT_PATH)
	if err:
		push_error("Failed to save version as script. Error: %s" % error_string(err))
	
	if FileAccess.file_exists("res://../MoonFlow.sln"):
		var simple_out := FileAccess.open("res://../version", FileAccess.WRITE)
		simple_out.store_string(version)
		simple_out.close()

## Stores the version in ProjectSettings.
func store_version_as_project_setting(version: String) -> void:
	if version.is_empty():
		printerr("Cannot store version. " + _EMPTY_VERSION_ERROR.format({"script_path": get_script().get_path()}))
		return
	
	if not ProjectSettings.has_setting(PROJECT_SETTING_NAME):
		ProjectSettings.set_initial_value(PROJECT_SETTING_NAME, "Empty version")
		ProjectSettings.add_property_info({
			"name": PROJECT_SETTING_NAME,
			"type": TYPE_STRING,
			"hint": PROPERTY_HINT_PLACEHOLDER_TEXT,
			"hint_string": "Will overriden on export by AutoExportVersion plugin"
		})
	
	ProjectSettings.set_setting(PROJECT_SETTING_NAME, version)
	ProjectSettings.save()


const _CURRENT_VERSION: String = "Current version: {version}"
const _EMPTY_VERSION_ERROR: String = "Version string is empty.\nMake sure your 'get_version()' in '{script_path}' is configured properly."

const _TOOL_MENU_ITEM_NAME: String = "AutoExport: Print and Update Current Version"

var _exporter: AutoExportVersionExporter

func _enter_tree() -> void:
	_exporter = AutoExportVersionExporter.new()
	_exporter.plugin = self
	add_export_plugin(_exporter)
	add_tool_menu_item(_TOOL_MENU_ITEM_NAME, _tool_menu_print_version)
	
	var setting_name := "addons/AutoExportVersion/version_store_location"
	if not ProjectSettings.has_setting(setting_name):
		ProjectSettings.set_setting(setting_name, STORE_LOCATION)
	ProjectSettings.add_property_info({ "name": setting_name, "type": TYPE_INT, "hint": PROPERTY_HINT_ENUM, "hint_string": "Script,Project Setting" })
	ProjectSettings.set_initial_value(setting_name, STORE_LOCATION)
	
	setting_name = "addons/AutoExportVersion/version_file_path"
	if not ProjectSettings.has_setting(setting_name):
		ProjectSettings.set_setting(setting_name, SCRIPT_PATH)
	ProjectSettings.add_property_info({ "name": setting_name, "type": TYPE_STRING, "hint": PROPERTY_HINT_SAVE_FILE })
	ProjectSettings.set_initial_value(setting_name, SCRIPT_PATH)
	
	setting_name = "addons/AutoExportVersion/version_setting_name"
	if not ProjectSettings.has_setting(setting_name):
		ProjectSettings.set_setting(setting_name, PROJECT_SETTING_NAME)
	ProjectSettings.set_initial_value(setting_name, PROJECT_SETTING_NAME)
	
	setting_name = "addons/AutoExportVersion/version_config_file"
	if not ProjectSettings.has_setting(setting_name):
		ProjectSettings.set_setting(setting_name, CONFIG_PATH)
		DirAccess.copy_absolute("res://addons/AutoExportVersion/auto_export_version_config_file.gd", CONFIG_PATH)
	ProjectSettings.add_property_info({ "name": setting_name, "type": TYPE_STRING, "hint": PROPERTY_HINT_SAVE_FILE })
	ProjectSettings.set_initial_value(setting_name, CONFIG_PATH)
	
	_sync_project_settings()
	ProjectSettings.settings_changed.connect(_sync_project_settings)
	
	if STORE_LOCATION == VersionStoreLocation.SCRIPT and not FileAccess.file_exists(SCRIPT_PATH):
		store_version_as_script(get_version(PackedStringArray(), true, "", 0))

func _sync_project_settings():
	STORE_LOCATION = ProjectSettings.get_setting("addons/AutoExportVersion/version_store_location")
	SCRIPT_PATH = ProjectSettings.get_setting("addons/AutoExportVersion/version_file_path")
	PROJECT_SETTING_NAME = ProjectSettings.get_setting("addons/AutoExportVersion/version_setting_name")
	
	var new_config_path: String = ProjectSettings.get_setting("addons/AutoExportVersion/version_config_file")
	if new_config_path != CONFIG_PATH:
		if FileAccess.file_exists(CONFIG_PATH):
			DirAccess.rename_absolute(CONFIG_PATH, new_config_path)
			EditorInterface.get_resource_filesystem().update_file(CONFIG_PATH)
			EditorInterface.get_resource_filesystem().update_file(new_config_path)
		
		CONFIG_PATH = new_config_path

func _exit_tree() -> void:
	remove_export_plugin(_exporter)
	remove_tool_menu_item(_TOOL_MENU_ITEM_NAME)

func _tool_menu_print_version() -> void:
	var version: String = get_version(PackedStringArray(), true, "", 0)
	
	if version.is_empty():
		printerr(_EMPTY_VERSION_ERROR.format({ "script_path": get_script().get_path() }))
		OS.alert(_EMPTY_VERSION_ERROR.format({ "script_path": get_script().get_path() }))
		return
	
	print(_CURRENT_VERSION.format({ "version": version }))
	OS.alert(_CURRENT_VERSION.format({ "version": version }))
	store_version(version, STORE_LOCATION)

func _update_version_in_editor() -> void:
	if !Engine.is_editor_hint():
		return
	
	var version: String = get_version(PackedStringArray(), true, "", 0)
	
	if version.is_empty():
		printerr(_EMPTY_VERSION_ERROR.format({ "script_path": get_script().get_path() }))
		OS.alert(_EMPTY_VERSION_ERROR.format({ "script_path": get_script().get_path() }))
		return
	
	print(_CURRENT_VERSION.format({ "version": version }))
	store_version(version, STORE_LOCATION)

func get_version(features: PackedStringArray, is_debug: bool, path: String, flags: int) -> String:
	if not ResourceLoader.exists(CONFIG_PATH, "GDScript"):
		push_error("Version config file does not exist!")
		return ""
	
	var provider: RefCounted = load(CONFIG_PATH).new()
	return provider.get_version(features, is_debug, path, flags)

class AutoExportVersionExporter extends EditorExportPlugin:
	var plugin: EditorPlugin
	
	func _get_name() -> String:
		return "Auto Export Version"

	func _export_begin(features: PackedStringArray, is_debug: bool, path: String, flags: int) -> void:
		if not plugin:
			push_error("No plugin set in AutoExportVersionExporter")
			return
		
		var version: String = plugin.get_version(features, is_debug, path, flags)
		plugin.store_version(version, plugin.STORE_LOCATION)
