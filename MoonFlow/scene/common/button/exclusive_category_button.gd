extends Button
class_name ExclusiveCategoryButton

@export var target: Control
@export var is_default_page: bool = false

enum FilterType
{
	CANVAS_ITEM,
	VBOX_CONTAINER,
}

@export var filter_type := FilterType.CANVAS_ITEM

func _ready() -> void:
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
	
	for button in get_parent().get_children():
		if button is Button:
			button.set_pressed_no_signal(button == self)
