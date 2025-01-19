extends MenuButton

var history: PackedStringArray = []

const max_list_size: int = 8
const max_str_len: int = 42

const setting_key: String = "moonflow/general/proj_history"

signal open_recent_project(proj_path: String)

func _ready() -> void:
	history = EngineSettings.get_setting(setting_key, [])
	if history.is_empty():
		hide()
		return
	
	# Limit list size
	if history.size() > max_list_size:
		history = history.slice(0, max_list_size)
	
	# Connect to events
	get_popup().index_pressed.connect(_on_item_selected)
	_init_button_options()

func _init_button_options() -> void:
	var popup := get_popup()
	popup.clear()
	
	# Initilize button options
	for item in history:
		var trim = item.substr(max(0, item.length() - max_str_len))
		if trim != item:
			trim = "..." + trim
		
		popup.add_item(trim)
	
	popup.add_separator()
	popup.add_item(tr("FRONT_DOOR_RECENT_PROJECTS_CLEAR_HISTORY"), 10000)

func _on_item_selected(idx: int) -> void:
	var id := get_popup().get_item_id(idx)
	if id == 10000:
		_clear_recent_history()
		return
	
	var sel: String = history[idx]
	history.remove_at(idx)
	
	open_recent_project.emit(sel)
	_init_button_options()

func _on_front_door_open_project(target: String) -> void:
	target = target.replace('\\', '/')
	target = target.trim_suffix('/')
	target = target.trim_suffix("romfs")
	
	if !target.ends_with('/'):
		target += '/'
	
	if history.has(target):
		return
	
	history.insert(0, target)
	if history.size() > max_list_size:
		history.slice(0, max_list_size)
	
	EngineSettings.set_setting(setting_key, history)
	EngineSettings.save()

func _on_front_door_open_project_failed(target: String) -> void:
	_on_front_door_open_project(target)
	_init_button_options()

func _clear_recent_history() -> void:
	history.clear()
	EngineSettings.set_setting(setting_key, history)
	EngineSettings.save()
	
	hide()
