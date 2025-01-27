extends Button
class_name ExclusiveCategoryButton

@export var target: Control
@export var parent_override: Control
@export var is_default_page: bool = false

enum FilterType
{
	CANVAS_ITEM,
	VBOX_CONTAINER,
}

@export var filter_type := FilterType.CANVAS_ITEM

func _ready() -> void:
	if parent_override == null:
		parent_override = get_parent()
	
	toggle_mode = true
	
	if is_default_page:
		_pressed()
		pressed.emit()

func _pressed() -> void:
	if is_instance_valid(target):
		for child in target.get_parent().get_children():
			if filter_type == FilterType.CANVAS_ITEM && child is CanvasItem:
				child.hide()
				continue
			
			if filter_type == FilterType.VBOX_CONTAINER && child is VBoxContainer:
				child.hide()
				continue
		
		target.show()
	
	_update_button_states(parent_override)

func _update_button_states(node: Node) -> void:
	for child in node.get_children():
		if child is Button:
			child.set_pressed_no_signal(child == self)
		
		if child.get_child_count() > 0:
			_update_button_states(child)
