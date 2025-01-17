extends Button

func _pressed() -> void:
	owner.queue_free()
