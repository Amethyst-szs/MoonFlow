extends VBoxContainer

@onready var root_container: PanelContainer = $"../../../"

func _ready() -> void:
	child_entered_tree.connect(_on_child_entered)

func _on_child_entered(_discard: Node) -> void:
	var sep := get_theme_constant("separation")
	var s: float = 0
	
	for node in get_children():
		if node is Control:
			s += node.size.y + sep
	
	root_container.custom_minimum_size.y = s
