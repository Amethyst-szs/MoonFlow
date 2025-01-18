extends Button

const wiki_default := preload("res://addons/wiki/paths/default.tres")

func _pressed() -> void:
	wiki_default.call("OpenWiki")
