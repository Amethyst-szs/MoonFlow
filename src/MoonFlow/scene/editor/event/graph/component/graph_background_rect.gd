extends ColorRect

@export var graph: CanvasLayer

var initial_dot_size: Vector2 = Vector2.ZERO

func _ready() -> void:
	var shader := material as ShaderMaterial
	initial_dot_size = shader.get_shader_parameter("dot_size")

func _process(_delta: float) -> void:
	var shader := material as ShaderMaterial
	shader.set_shader_parameter("graph_position", -graph.offset)
	shader.set_shader_parameter("dot_size", initial_dot_size * graph.scale)
