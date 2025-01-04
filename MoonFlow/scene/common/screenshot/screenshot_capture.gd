extends Node

const export_path_base: String = "user://capture/"

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_screenshot", false, true):
		var tex := get_viewport().get_texture()
		var img := tex.get_image()
		
		var datetime := Time.get_datetime_string_from_system()
		datetime = datetime.replace('T', '-').replace(':', '')
		var path := export_path_base + "MoonFlow-" + datetime + ".png"
		
		DirAccess.make_dir_absolute(export_path_base)
		img.save_png(path)
		print("Saved screenshot to " + path)
