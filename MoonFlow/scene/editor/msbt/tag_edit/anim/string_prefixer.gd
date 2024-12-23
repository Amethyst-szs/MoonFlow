extends RichTextLabel

@export var prefix: String = ""

func _ready() -> void:
	text = prefix + tr(text)
