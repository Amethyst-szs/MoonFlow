extends VBoxContainer

const screen_margin_tl := Vector2(28, 138)
const screen_margin := Vector2(28, 42)
const step_s: float = 140.0
const step_e: float = 270.0

const text_margin := Vector2(12, 12)

@onready var parent: PanelContainer = $"../../../"

func _process(_delta: float) -> void:
	var canvas := get_canvas_layer_node()
	if !is_instance_valid(canvas): return
	
	var root_screen_pos: Vector2 = get_parent().get_screen_position()
	
	# Calculate screen margin step
	var step_pos := clampf((root_screen_pos.x - step_s) / (step_e - step_s), 0, 1)
	var stepped := Vector2.ZERO
	stepped.x = lerpf(screen_margin_tl.x, screen_margin.x, step_pos)
	stepped.y = lerpf(screen_margin_tl.y, screen_margin.y, step_pos)
	
	# Calculate label position
	root_screen_pos -= stepped
	root_screen_pos *= Vector2.ONE / canvas.scale
	
	_update_x(-root_screen_pos.x)
	_update_y(-root_screen_pos.y)

func _update_x(screen: float) -> void:
	if screen < 0:
		position.x = 0
		return
	
	var sm := size.x + text_margin.x
	position.x = clampf(screen, 0, parent.size.x - sm)

func _update_y(screen: float) -> void:
	if screen < 0:
		position.y = 0
		return
	
	var sm := size.y + text_margin.y
	position.y = clampf(screen, 0, parent.size.y - sm)
