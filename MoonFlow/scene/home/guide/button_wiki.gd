extends Button

const wiki_default := preload("res://scene/common/wiki/paths/default.tres")

func _pressed() -> void:
	wiki_default.call("OpenWiki")
