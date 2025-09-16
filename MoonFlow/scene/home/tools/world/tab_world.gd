extends Control

const earth_scene := preload("uid://cjuty71e2t356")

func _ready():
	if !EngineSettings.get_setting("moonflow/general/world_list_shader", true):
		return
	
	var earth := earth_scene.instantiate()
	add_child(earth)
	move_child(earth, 0)
