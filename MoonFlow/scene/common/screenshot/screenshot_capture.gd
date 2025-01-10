extends Node

var export_content: Image = null

@onready var dialog := FileDialog.new()

func _ready() -> void:
	dialog.file_mode = FileDialog.FILE_MODE_SAVE_FILE
	dialog.title = tr("SCREENSHOT_FILE_DIALOG_TITLE")
	dialog.use_native_dialog = true
	dialog.access = FileDialog.ACCESS_FILESYSTEM
	
	dialog.file_selected.connect(_on_dialog_accepted)
	
	add_child(dialog)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_screenshot", false, true):
		var tex := get_viewport().get_texture()
		export_content = tex.get_image()
		
		var datetime := Time.get_datetime_string_from_system()
		datetime = datetime.replace('T', '-').replace(':', '')
		var target := "MoonFlow-" + datetime + ".png"
		
		dialog.current_file = target
		dialog.popup()

func _on_dialog_accepted(path: String) -> void:
	export_content.save_png(path)
	export_content = null
	
	print("Saved screenshot to " + path)
