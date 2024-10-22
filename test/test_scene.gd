extends Node

const byml: CSharpScript = preload("res://addons/nindot/src/Byml.cs")

# Called when the node enters the scene tree for the first time.
func _ready():
	byml.Test();


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
