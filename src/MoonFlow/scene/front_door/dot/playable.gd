extends AnimatedSprite2D

var velocity: Vector2 = Vector2.ZERO
var velocity_jump_start: Vector2 = Vector2.ZERO

var input_dir: float = 0.0

var is_jump_input: bool = false
var is_jump_hold: bool = false
var is_mouse_toy: bool = false

var is_appear: bool = false
var is_dead: bool = false
var death_delay_time: int = 0

@export var appear_delay_sec: float = 15.0

#region Animation

func _ready() -> void:
	# Hide the object for appear delay time
	hide()
	await get_tree().create_timer(appear_delay_sec).timeout
	
	# Play appear animation
	show()
	is_appear = true
	
	position = Vector2(0, sprite_size.y)
	velocity = Vector2(1.4, -4.5)
	
	await get_tree().create_timer(0.4).timeout
	is_appear = false

func _process(_delta: float) -> void:
	if !visible: return
	
	# Set facing direction
	if velocity.sign().x != 0:
		flip_h = velocity.x < 0
	
	if is_dead:
		play("dead")
		return
	
	# Set SpriteFrames animation
	if is_airborne():
		play("jump")
		return
	
	if velocity.x == 0:
		play("idle")
		return
	
	if is_skid():
		play("skid")
		return
	
	speed_scale = absf(velocity.x) / max_velocity_h
	
	if animation == "run":
		return
	
	play("run")

#endregion

#region Input

func _input(event: InputEvent) -> void:
	if !visible: return
	
	if event.is_action_pressed("dot_playable_up") and !is_airborne():
		is_jump_input = true
		is_jump_hold = true
	
	if event.is_action_released("dot_playable_up"):
		is_jump_hold = false
	
	if event is InputEventMouseButton and event.is_pressed():
		if !is_dead and sprite_rect.has_point(get_local_mouse_position()):
			death_delay_time = 60
			is_dead = true

#endregion

#region Physics Constants and Calculations

const accel_h: float = 0.08941
const decel_h: float = 0.08156
const decel_skid_h: float = 0.1631

const midair_h_threshold: float = 1.5686
const midair_high_speed_decel_h: float = 0.08941
const midair_low_speed_decel_h: float = 0.08156
const midair_high_speed_accel_h: float = 0.08941
const midair_low_speed_accel_h: float = 0.05960

const max_velocity_h: float = 2.5625

const initial_velocity_low_v: float = -4.0
const initial_velocity_high_v: float = -5.0
const initial_velocity_v_threshold_h: float = 2.32156

const max_velocity_v: float = 4.5019

const mouse_toy_range: float = 75
const sprite_size := Vector2(16, 32)
const sprite_rect := Rect2(Vector2.ZERO, sprite_size)

func _physics_process(_delta: float) -> void:
	if !visible: return
	
	# If dead, jump to death routine
	if is_dead:
		_process_dead()
		return
	
	# Update input direction
	var is_key_input: bool = false
	input_dir = 0.0
	
	if Input.is_action_pressed("dot_playable_left"):
		input_dir -= 1
		is_key_input = true
	
	if Input.is_action_pressed("dot_playable_right"):
		input_dir += 1
		is_key_input = true
	
	# Update mouse toy state
	var mouse_dist := (get_local_mouse_position() - (sprite_size / 2)).distance_to(Vector2.ZERO)
	is_mouse_toy = mouse_dist < mouse_toy_range && !is_key_input
	
	# Calculate horizontal velocity
	if is_mouse_toy:
		_process_mouse_toy_accel()
	else: if input_dir == 0:
		_process_no_input_decel()
	else: if is_skid():
		_process_skid_decel()
	else:
		_process_accel()
	
	# Calculate vertical velocity
	if is_jump_input && !is_airborne():
		_process_jump_start()
	
	if is_airborne():
		_process_airborne()
	
	# Update position
	position += Vector2(velocity.x, velocity.y)
	
	# Clamp player above the floor
	if is_below_floor() and !is_appear:
		position.y = 0.0
		velocity.y = 0.0
	
	# Clamp player within the window bounds
	var origin = get_parent().global_position.x
	var origin_s = get_parent().scale.x
	var halfWin = float(get_window().size.x) / 2
	global_position.x = clampf(global_position.x, -halfWin + origin, halfWin + origin - (sprite_size.x * origin_s))

#endregion

#region Horizontal Physics Process Utilities

func _process_no_input_decel() -> void:
	if !is_airborne():
		velocity.x = clampf(absf(velocity.x) - decel_h, 0, max_velocity_h) * get_sign_h()
		return
	
	return # Velocity is unchanged with no input while airborne

func _process_skid_decel() -> void:
	if !is_airborne():
		velocity.x = clampf(absf(velocity.x) - decel_skid_h, 0, max_velocity_h) * get_sign_h()
		return
	
	if absf(velocity.x) >= midair_h_threshold:
		velocity.x = clampf(absf(velocity.x) - midair_high_speed_decel_h, 0, max_velocity_h) * get_sign_h()
	else:
		velocity.x = clampf(absf(velocity.x) - midair_low_speed_decel_h, 0, max_velocity_h) * get_sign_h()

func _process_accel() -> void:
	var s = get_sign_h()
	
	if is_mouse_toy:
		s = get_mouse_toy_sign_h()
	
	if !is_airborne():
		velocity.x = clampf(velocity.x + (accel_h * s), -max_velocity_h, max_velocity_h)
		return
	
	if absf(velocity.x) >= midair_h_threshold:
		velocity.x = clampf(velocity.x + (midair_high_speed_accel_h * s), -max_velocity_h, max_velocity_h)
	else:
		velocity.x = clampf(velocity.x + (midair_low_speed_accel_h * s), -max_velocity_h, max_velocity_h)

func _process_mouse_toy_accel() -> void:
	var mouse := get_local_mouse_position()
	var s := signf(mouse.x)
	
	input_dir = (mouse.distance_to(Vector2.ZERO) / mouse_toy_range) * -s
	_process_accel()

#endregion

#region Vertical Physics Process Utilities

func _process_jump_start() -> void:
	is_jump_input = false
	
	if absf(velocity.x) > initial_velocity_v_threshold_h:
		velocity.y = initial_velocity_high_v
	else:
		velocity.y = initial_velocity_low_v
	
	velocity_jump_start = velocity

func _process_airborne() -> void:
	# Update velocity
	velocity.y += calc_gravity()
	
	# Cap downward velocity
	if velocity.y > max_velocity_v:
		velocity.y = 4.0

#endregion

#region Special Physics Process Utilities

func _process_dead() -> void:
	if death_delay_time > 0:
		velocity = Vector2.ZERO
		death_delay_time -= 1
		return
	
	if death_delay_time == 0:
		_process_jump_start()
		death_delay_time = -1
		return
	
	_process_airborne()
	position.y += velocity.y
	
	if position.y > 100:
		position = Vector2(0, -get_window().size.y)
		is_dead = false

#endregion

#region Calc Utilities

func get_sign_h() -> float:
	var s = velocity.sign().x
	if s == 0.0:
		return input_dir
	
	return s

func get_mouse_toy_sign_h() -> float:
	if !is_mouse_toy:
		return 1
	
	return -signf(get_local_mouse_position().x)

func calc_gravity() -> float:
	if is_jump_hold || is_dead || is_appear:
		return _calc_gravity_jump_hold()
	else:
		return _calc_gravity_jump_released()

func _calc_gravity_jump_hold() -> float:
	if absf(velocity_jump_start.x) < 1.0:
		return 0.1254
	
	if absf(velocity_jump_start.x) > 2.3137:
		return 0.1568
	
	return 0.1176

func _calc_gravity_jump_released() -> float:
	if absf(velocity_jump_start.x) < 1.0:
		return 0.4392
	
	if absf(velocity_jump_start.x) > 2.3137:
		return 0.5647
	
	return 0.3764

func is_airborne() -> bool:
	return position.y < 0 || is_appear

func is_below_floor() -> bool:
	return position.y > 0

func is_skid() -> bool:
	return input_dir == -velocity.sign().x

#endregion
