extends MenuButton

signal selected_copy_to_project
signal selected_trash

func _ready() -> void:
	get_popup().id_pressed.connect(_on_id_pressed)

func _on_id_pressed(id: int) -> void:
	match(id):
		0:
			selected_copy_to_project.emit()
		1:
			selected_trash.emit()
		_:
			print("Invalid menu selection ID")
