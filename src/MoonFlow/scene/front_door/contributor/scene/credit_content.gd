extends VBoxContainer

const data: ContributorResourceList = preload("res://scene/front_door/contributor/resource_list.tres")
const scene: PackedScene = preload("res://scene/front_door/contributor/scene/category.tscn")

func _ready():
	for item in data.resource_list:
		var s := scene.instantiate()
		s.setup(item)
		add_child(s)
