extends Node

var cfg := ConfigFile.new()

const path: String = "res://override.cfg"

func _enter_tree() -> void:
	Engine.register_singleton("EngineSettingsCSharp", self)
	
	if FileAccess.file_exists(path):
		cfg.load(path)
	
	TranslationServer.set_locale(get_setting("moonflow/general/locale", "en"))

func _exit_tree() -> void:
	save()

func get_setting(key: String, default: Variant) -> Variant:
	var lookup := key.split('/', true, 1)
	if cfg.has_section_key(lookup[0], lookup[1]):
		return cfg.get_value(lookup[0], lookup[1], default)
	
	return ProjectSettings.get_setting(key, default)

func set_setting(key: String, value: Variant) -> void:
	var cfg_access := key.split('/', true, 1)
	
	cfg.set_value(cfg_access[0], cfg_access[1], value)
	ProjectSettings.set_setting(key, value)

func remove_setting(key: String) -> void:
	var cfg_access := key.split('/', true, 1)
	
	if cfg.has_section_key(cfg_access[0], cfg_access[1]):
		cfg.erase_section_key(cfg_access[0], cfg_access[1])

func save() -> void:
	cfg.save(path)

#region Signals

signal taskbar_size_modified()

func _taskbar_size_modified() -> void:
	taskbar_size_modified.emit()

#endregion

#region Utility

func get_wiki() -> String:
	var is_local: bool = get_setting("moonflow/wiki/is_use_local", true)
	
	if is_local:
		return get_wiki_local()
	else:
		return get_wiki_remote()

func get_wiki_local() -> String:
	return get_setting("moonflow/wiki/local_source",
		"res://docs/")

func get_wiki_remote() -> String:
	return get_setting("moonflow/wiki/remote_source",
		"https://github.com/Amethyst-szs/MoonFlow/tree/stable/docs/")

#endregion
