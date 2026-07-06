extends ColorRect

@export var graph: CanvasLayer

var initial_dot_size: Vector2 = Vector2.ONE * 45.0

const disable_key: String = "moonflow/event_graph/disable_background"
const parallax_key: String = "moonflow/event_graph/is_parallax_background"

func _enter_tree() -> void:
	if EngineSettings.get_setting(disable_key, false):
		material = null
		set_script(null)

func _process(_delta: float) -> void:
	if !is_instance_valid(material):
		return
	
	var shader := material as ShaderMaterial
	shader.set_shader_parameter("graph_position", -graph.offset)
	shader.set_shader_parameter("dot_size", initial_dot_size * graph.scale)
	
	if EngineSettings.get_setting(parallax_key, true):
		shader.set_shader_parameter("parallax", 0.9)
	else:
		shader.set_shader_parameter("parallax", 1.0)
