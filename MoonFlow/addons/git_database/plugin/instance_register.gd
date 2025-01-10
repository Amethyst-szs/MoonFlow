extends Node

func _enter_tree() -> void:
	Engine.register_singleton("GitInfoInst", GitInfo.new())

func _exit_tree() -> void:
	Engine.unregister_singleton("GitInfoInst")
