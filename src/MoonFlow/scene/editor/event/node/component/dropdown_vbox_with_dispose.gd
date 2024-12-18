extends VBoxContainer

@export var dropdown: CheckBox

func _ready() -> void:
	tree_exiting.connect(_on_leave)
	child_exiting_tree.connect(_on_child_leave)

func _on_leave() -> void:
	dropdown.queue_free()

func _on_child_leave(_child: Node) -> void:
	await Engine.get_main_loop().process_frame
	
	if get_child_count() != 0:
		return
	
	queue_free()
