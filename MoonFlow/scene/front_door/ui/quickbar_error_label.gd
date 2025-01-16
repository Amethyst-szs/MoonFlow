extends Label

var INVALID_PATH: String = tr("INVALID_PATH", "FRONT_DOOR_OPEN_ERROR")
var NO_PROJECT_FILE: String = tr("NO_PROJECT_FILE", "FRONT_DOOR_OPEN_ERROR")
var INVALID_PROJECT_FILE: String = tr("INVALID_PROJECT_FILE", "FRONT_DOOR_OPEN_ERROR")
var ROMFS_MISSING_PATH_FOR_PROJECT_VERSION: String = tr("ROMFS_MISSING_PATH_FOR_PROJECT_VERSION", "FRONT_DOOR_OPEN_ERROR")

var PROJECT_FILE_ALREADY_EXISTS: String = tr("PROJECT_FILE_ALREADY_EXISTS", "FRONT_DOOR_OPEN_ERROR")

func _ready() -> void:
	modulate = Color.TRANSPARENT

func _process(_delta: float) -> void:
	visible = (modulate != Color.TRANSPARENT)

func set_label(key: String, version: String) -> void:
	modulate = Color.WHITE
	text = get(key)
	if key == "ROMFS_MISSING_PATH_FOR_PROJECT_VERSION":
		text += version
	
	# Play disappear animation after delay
	var tween = create_tween().set_ease(Tween.EASE_IN).set_trans(Tween.TRANS_QUINT)
	tween.tween_property(self, "modulate", Color.TRANSPARENT, 5.0)
