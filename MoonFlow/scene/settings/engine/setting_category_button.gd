extends Button

@export var target: Control
@export var is_default_page: bool = false

func _ready() -> void:
	if is_default_page:
		_pressed()

func _pressed() -> void:
	for child in target.get_parent().get_children():
		if child is CanvasItem:
			child.hide()
	
	for button in get_parent().get_children():
		if button is Button:
			button.set_pressed_no_signal(button == self)
	
	target.show()
