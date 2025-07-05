extends Button
class_name DoubleClickButton

var timer: Timer = null

signal double_pressed

func _ready() -> void:
	action_mode = ACTION_MODE_BUTTON_PRESS

func _pressed() -> void:
	if timer:
		timer.queue_free()
		timer = null
		
		double_pressed.emit()
		return
	
	timer = Timer.new()
	timer.wait_time = 0.22
	timer.one_shot = true
	timer.autostart = true
	timer.timeout.connect(_on_timer_timeout)
	
	add_child(timer)

func _on_timer_timeout() -> void:
	if timer:
		timer.queue_free()
		timer = null
