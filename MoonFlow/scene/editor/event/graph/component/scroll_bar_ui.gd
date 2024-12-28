extends Control
class_name EventFlowGraphScrollNavigationBars

@onready var hbar: HScrollBar = $HScroll
@onready var vbar: VScrollBar = $VScroll

@export var graph: CanvasLayer
@export var node_holder: Node2D

var node_extent_rect := Rect2()
var graph_scale := Vector2.ONE

var rate_limit := Timer.new()

func _ready() -> void:
	# Setup rate limiter
	rate_limit.one_shot = true
	rate_limit.wait_time = 0.1
	add_child(rate_limit)
	
	# Setup window size events
	get_window().size_changed.connect(_on_window_size_changed)
	_on_window_size_changed()
	
	# Connect to scrolling events
	hbar.scrolling.connect(_on_scrolling_hbar)
	vbar.scrolling.connect(_on_scrolling_vbar)

func _on_scrolling_hbar() -> void:
	graph.offset.x = -hbar.value * (1.0 / graph_scale.x)

func _on_scrolling_vbar() -> void:
	graph.offset.y = -vbar.value * (1.0 / graph_scale.y)

#region Extent Calculation

func _on_graph_node_added(node: Node) -> void:
	if node.has_signal("NodeMoved") and node.has_method("GetRect"):
		node.connect("NodeMoved", _calculate_extents)

func _calculate_extents(ignore_rate_limit: bool = false) -> void:
	if !rate_limit.is_stopped() and !ignore_rate_limit:
		return
	
	rate_limit.start()
	
	# Calculate graph bounds
	var rect := Rect2()
	for node in node_holder.get_children():
		if node.has_method("GetRect"):
			rect = rect.merge(node.call("GetRect"))
	
	# Add margin to rect
	rect = rect.grow(60.0)
	
	node_extent_rect = rect
	
	# Update scrollbar ranges
	var win_size: Vector2 = get_window().size
	
	hbar.min_value = minf(rect.position.x, 0)
	hbar.max_value = maxf(rect.end.x, win_size.x)
	
	vbar.min_value = minf(rect.position.y, 0)
	vbar.max_value = maxf(rect.end.y, win_size.y)

func _on_window_size_changed() -> void:
	var win_size: Vector2i = Vector2(get_window().size) * graph_scale
	hbar.page = win_size.x
	vbar.page = win_size.y
	
	_calculate_extents(true)

func _on_graph_offset_changed(pos: Vector2, s: Vector2) -> void:
	hbar.value = -pos.x
	vbar.value = -pos.y
	
	graph_scale = s
	_on_window_size_changed()

#endregion
