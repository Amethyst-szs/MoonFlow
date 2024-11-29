extends PanelContainer

var shader: ShaderMaterial = null
var angle: float = PI / 2

const range_of_motion: float = 0.33
const lerp_speed: float = 0.01

func _ready() -> void:
	shader = material

func _process(_delta: float) -> void:
	var win: Vector2 = get_window().size
	var mouse: Vector2 = get_global_mouse_position()
	mouse.x = clampf(mouse.x, 0, win.x)
	mouse.y = win.y - clampf(mouse.y, 0, win.y)
	
	var hFac := (mouse.x / win.x) * 2 - 1
	var newAngle := ((win.y / 2 - mouse.y) / win.y) * 2 * hFac
	newAngle *= range_of_motion
	newAngle += PI / 2
	
	angle = lerpf(angle, newAngle , lerp_speed)
	shader.set_shader_parameter("color_angle", angle)
