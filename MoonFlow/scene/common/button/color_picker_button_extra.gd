class_name ColorPickerButtonEx
extends ColorPickerButton

@export var init_color_mode := ColorPicker.ColorModeType.MODE_RGB
@export var init_picker_shape := ColorPicker.PickerShapeType.SHAPE_HSV_RECTANGLE
@export var init_can_add_swatches: bool = true

@export_group("Customization")
@export var init_sampler_visible: bool = true
@export var init_color_modes_visible: bool = true
@export var init_sliders_visible: bool = true
@export var init_hex_visible: bool = true
@export var init_presets_visible: bool = true

func _ready() -> void:
	var picker := get_picker()
	picker.color_mode = init_color_mode
	picker.picker_shape = init_picker_shape
	picker.can_add_swatches = init_can_add_swatches
	picker.sampler_visible = init_sampler_visible
	picker.color_modes_visible = init_color_modes_visible
	picker.sliders_visible = init_sliders_visible
	picker.hex_visible = init_hex_visible
	picker.presets_visible = init_presets_visible
