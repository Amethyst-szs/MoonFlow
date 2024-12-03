extends Button
class_name DoubleClickButton

var timer: Timer = null

signal double_pressed

func _ready() -> void:
	action_mode = ACTION_MODE_BUTTON_PRESS
	pressed.connect(_on_pressed)

func _exit_tree() -> void:
	for con in pressed.get_connections(): pressed.disconnect(con["callable"])
	for con in double_pressed.get_connections(): double_pressed.disconnect(con["callable"])

func _on_pressed() -> void:
	if timer:
		timer.timeout.disconnect(_on_timer_timeout)
		remove_child(timer)
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
		remove_child(timer)
		timer = null
