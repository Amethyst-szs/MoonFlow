extends MenuButton

## Must be defined in order to store the state of the filter
## menu between sessions
@export var identity: String = ""

signal filter_by_modified(state: bool)

var id_mapping: Dictionary[int, Signal] = {
	0: filter_by_modified
}

func _ready() -> void:
	var popup := get_popup()
	popup.id_pressed.connect(_on_id_pressed)
	
	# Get states from engine settings
	if identity.is_empty():
		push_warning("Identity is empty in filter button node!")
		return
	
	for item in popup.item_count:
		var id := popup.get_item_id(item)
		var state: bool = EngineSettings.get_setting(_calc_setting_path(id), false)
		
		popup.set_item_checked(item, state)
		id_mapping[id].emit(state)

func _on_id_pressed(id: int) -> void:
	var popup := get_popup()
	var idx := popup.get_item_index(id)
	
	var state := popup.is_item_checked(idx)
	id_mapping[id].emit(state)
	
	# Update engine settings save data
	EngineSettings.set_setting(_calc_setting_path(id), state)

func _calc_setting_path(id: int) -> String:
	return "filter_state/%s/%d" % [identity, id]
