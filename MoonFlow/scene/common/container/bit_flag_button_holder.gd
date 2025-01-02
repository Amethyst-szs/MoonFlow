@tool
extends VBoxContainer
class_name BitFlagButtonHolder

@export_group("Bit Selection")
@export var allow_bit_selection: bool = true
@export var value: int = 0

@export_group("Primary Bit Selection")
@export var allow_primary_bit_selection: bool = false
@export var primary_bit: int = -1
@export var primary_bit_icon: Texture2D

@export_group("Button Layout")
@export var total_bits: int = 1
@export var rows: int = 1
@export var button_size := Vector2(32, 32)

var bit_buttons: Array[Button] = []
var editor_timer: Timer = null

const theme_inst: Theme = preload(
	"res://asset/theme/common/common_bit_flag_container.tres"
)

signal value_changed(value: int)
signal primary_bit_changed(value: int)

func _ready():
	theme = theme_inst
	_setup()
	
	if Engine.is_editor_hint():
		editor_timer = Timer.new()
		editor_timer.wait_time = 1.0
		editor_timer.one_shot = true
		add_child(editor_timer)

func set_value(v: int) -> void:
	value = v
	
	for bit in bit_buttons:
		bit.set_pressed_no_signal(value & 1 << int(bit.get_meta("id")))

func set_primary_bit(bit: int) -> void:
	primary_bit = bit
	for button in bit_buttons:
		if !allow_bit_selection:
			button.set_pressed_no_signal(false)
		
		button.icon = null
	
	if bit >= 0 && bit < bit_buttons.size():
		bit_buttons[bit].icon = primary_bit_icon
		if !allow_bit_selection:
			bit_buttons[bit].set_pressed_no_signal(true)

func _setup() -> void:
	if Engine.is_editor_hint() && is_instance_valid(editor_timer):
		if !editor_timer.is_stopped(): return
		editor_timer.start()
	
	for bit in bit_buttons:
		if is_instance_valid(bit):
			bit.queue_free()
	
	bit_buttons.clear()
	
	var bits_per_row: int = int(float(total_bits) / float(rows))
	var box: HBoxContainer = null
	
	var row_count: int = 0
	for i in range(total_bits):
		if i % bits_per_row == 0 && row_count < rows:
			if box: add_child(box)
			
			box = HBoxContainer.new()
			box.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
			
			row_count += 1
		
		var button := Button.new()
		button.set_meta("id", i)
		
		button.custom_minimum_size = button_size
		button.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
		button.icon_alignment = HORIZONTAL_ALIGNMENT_RIGHT
		button.text = str(i + 1)
		button.toggle_mode = true
		button.action_mode = BaseButton.ACTION_MODE_BUTTON_PRESS
		button.button_mask = MOUSE_BUTTON_MASK_LEFT | MOUSE_BUTTON_MASK_RIGHT
		
		button.pressed.connect(_on_bit_pressed.bind(i))
		
		bit_buttons.append(button)
		box.add_child(button)
	
	if !box.is_inside_tree():
		add_child(box)
	
	if allow_bit_selection: set_value(value)
	if allow_primary_bit_selection: set_primary_bit(primary_bit)

func _on_bit_pressed(bit: int) -> void:
	var mask := Input.get_mouse_button_mask()
	
	if allow_bit_selection && allow_primary_bit_selection:
		if mask == MOUSE_BUTTON_MASK_LEFT:
			_on_bit_pressed_update_value(bit)
		else:
			_on_bit_pressed_update_primary(bit)
	
	if allow_bit_selection: _on_bit_pressed_update_value(bit)
	if allow_primary_bit_selection: _on_bit_pressed_update_primary(bit)

func _on_bit_pressed_update_value(bit: int) -> void:
	value ^= 1 << bit
	value_changed.emit(value)

func _on_bit_pressed_update_primary(bit: int) -> void:
	if primary_bit == bit:
		bit = -1
	
	set_primary_bit(bit)
	primary_bit_changed.emit(bit)

func _validate_property(_property) -> void:
	_setup()
