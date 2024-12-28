extends Node2D

@export var miniline_count: int = 10

var canvas: CanvasLayer = null
var render_offset := Vector2.ZERO

func _ready() -> void:
	var scan = self
	while !canvas:
		if scan is CanvasLayer:
			canvas = scan
			return
		
		if !scan:
			push_error("Canvas lookup failed!")
			return
		
		scan = scan.get_parent()

func _process(_delta: float) -> void:
	render_offset = -canvas.offset * (Vector2.ONE / canvas.scale)
	queue_redraw()

func _draw() -> void:
	var factorf := 1.0 / canvas.scale.x
	var factor := Vector2.ONE / canvas.scale
	var target := Vector2(get_window().size) * factor
	
	var col := Color.WEB_GRAY
	draw_line(Vector2(render_offset.x, 0), Vector2(render_offset.x + target.x, 0), col, 4 * factorf)
	draw_line(Vector2(0, render_offset.y), Vector2(0, render_offset.y + target.y), col, 4 * factorf)
	draw_circle(Vector2.ZERO, 8 * factorf, Color.DARK_GRAY)
