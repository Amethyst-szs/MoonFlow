extends MenuButton

# Color names to hue position
const ITEM_TABLE: Dictionary[String, float] = {
	"Red": 0.0,
	"Orange": 24.0,
	"Yellow": 51.0,
	"Green": 123.0,
	"Blue": 196.0,
	"Purple": 260.0,
	"Pink": 320.0,
}

signal color_selected(hue: float)

func _ready() -> void:
	var menu: PopupMenu = get_popup()
	menu.index_pressed.connect(_on_index_pressed)
	
	for item in ITEM_TABLE.keys():
		menu.add_icon_item(null, item)

func _on_index_pressed(idx: int) -> void:
	color_selected.emit(ITEM_TABLE.values()[idx])
