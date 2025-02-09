@tool
extends TextureRect

var current_rotation := Quaternion(Vector3(1, 0, 0), PI)
var base_scale := Vector2.ONE
var base_win_size := Vector2.ONE

@export var velocity_div: float = 1.0

@export var velocity: float = 0.0
@export var speed: float = 0.0

@export var friction: float = 1.0
@export var idle_speed: float = 1.0
@export var max_speed: float = 1.0

func _ready() -> void:
	base_scale = scale
	base_win_size.x = ProjectSettings.get_setting("display/window/size/viewport_width")
	base_win_size.y = ProjectSettings.get_setting("display/window/size/viewport_height")
	
	get_window().size_changed.connect(_on_window_size_changed)
	_on_window_size_changed()
	
	_update_shader()

func _on_window_size_changed() -> void:
	var ratio := Vector2(get_window().size) / base_win_size
	var ratio_max := maxf(ratio.x, ratio.y)
	scale = base_scale * Vector2(ratio_max, ratio_max)

func _input(event: InputEvent) -> void:
	if !visible: return
	if event is InputEventMouseMotion:
		velocity = event.screen_velocity.x / velocity_div

func _process(delta: float) -> void:
	if !visible: return
	
	var s := signf(speed)
	var v := (velocity * delta)
	var f := (friction * delta)
	
	speed = clampf(speed + v + (f * -s), -max_speed, max_speed)
	
	if s == 0: s = -1
	
	if signf(velocity) == 0 || signf(velocity) == s:
		if absf(speed) < idle_speed:
			speed = idle_speed * s
	
	if velocity != 0.0: velocity = 0.0
	
	var distance = -speed
	var quat: Quaternion = Quaternion(Vector3(0, 1, 0), distance)
	
	current_rotation = current_rotation * quat
	current_rotation = current_rotation.normalized()
	
	_update_shader()

func _update_shader() -> void:
	var mat := material as ShaderMaterial
	mat.set_shader_parameter("quaternion", current_rotation)
