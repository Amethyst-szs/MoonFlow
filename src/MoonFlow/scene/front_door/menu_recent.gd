extends MenuButton

var history: PackedStringArray = []

const max_list_size: int = 8
const max_str_len: int = 32
const path: String = "user://proj.history"

signal open_recent_project(proj_path: String)

func _ready() -> void:
	# Read history file if it exists
	if !FileAccess.file_exists(path):
		hide()
		return
	
	var file := FileAccess.get_file_as_string(path)
	history = file.split("\n", false)
	
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
	get_popup().clear()
	
	# Initilize button options
	for item in history:
		var trim = item.substr(max(0, item.length() - max_str_len))
		if trim != item:
			trim = "..." + trim
		
		get_popup().add_item(trim)

func _on_item_selected(idx: int) -> void:
	var sel: String = history[idx]
	
	history.remove_at(idx)
	history.insert(0, sel)
	_init_button_options()
	
	open_recent_project.emit(sel)

func _on_front_door_open_project(target: String) -> void:
	if history.has(target):
		return
	
	history.insert(0, target)
	if history.size() > max_list_size:
		history.slice(0, max_list_size)
	
	var outstr: String = ""
	for item in history:
		outstr += item + '\n'
	
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		push_error(FileAccess.get_open_error())
		return
	
	file.store_string(outstr)
	file.close()
