extends VBoxContainer

@export var dropdown: CheckBox

func _ready() -> void:
	visibility_changed.connect(_on_visiblity_changed)
	tree_exiting.connect(_on_leave)
	child_exiting_tree.connect(_on_child_leave)

func _on_visiblity_changed() -> void:
	await Engine.get_main_loop().process_frame
	
	dropdown.visible = get_child_count() != 0
	
	if visible != dropdown.button_pressed:
		visible = dropdown.button_pressed

func _on_leave() -> void:
	if !dropdown.is_queued_for_deletion():
		dropdown.queue_free()

func _on_child_leave(_child: Node) -> void:
	await Engine.get_main_loop().process_frame
	
	if get_child_count() != 0:
		return;
	
	hide()
