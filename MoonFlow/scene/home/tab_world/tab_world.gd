extends Control

const earth_path: String = "res://scene/home/tab_world/earth/earth_holder.tscn"

func _ready():
	if !EngineSettings.get_setting("moonflow/general/world_list_shader", true):
		return
	
	var earth_scene: PackedScene = load(earth_path)
	var earth := earth_scene.instantiate()
	
	add_child(earth)
	move_child(earth, 0)
