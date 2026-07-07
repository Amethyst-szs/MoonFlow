extends HBoxContainer

signal name_modified(source: Node, new_name: String)
signal color_top_picker_request(source: Node)
signal color_bottom_picker_request(source: Node)

@onready var line: LineEdit = $Panel_Color/Layout/VBox_Config/Line_ColorName
@onready var gradiant_check: CheckBox = $Panel_Color/Layout/VBox_Config/Check_GradiantMode
@onready var picker_top: Button = $Panel_Color/Layout/VBox_Pickers/Button_PickerTop
@onready var picker_bottom: Button = $Panel_Color/Layout/VBox_Pickers/Button_PickerBottom

@onready var preview_b: RichTextLabel = $Panel_PreviewBlack/Preview
@onready var preview_w: RichTextLabel = $Panel_PreviewWhite/Preview

func setup(n: String, top: Color, bottom: Color, is_gradiant: bool) -> void:
	name = n
	line.text = n
	gradiant_check.button_pressed = is_gradiant
	picker_bottom.visible = is_gradiant
	
	preview_b.material = preview_b.material.duplicate()
	preview_w.material = preview_w.material.duplicate()
	
	set_colors(top, bottom)

func set_colors(top: Color, bottom: Color) -> void:
	picker_top.self_modulate = top
	picker_bottom.self_modulate = bottom
	
	(preview_b.material as ShaderMaterial).set_shader_parameter("first_color", top)
	(preview_w.material as ShaderMaterial).set_shader_parameter("first_color", top)
	(preview_b.material as ShaderMaterial).set_shader_parameter("second_color", bottom)
	(preview_w.material as ShaderMaterial).set_shader_parameter("second_color", bottom)

func is_gradient_mode() -> bool:
	return gradiant_check.button_pressed

func _on_color_top_picker_request() -> void:
	color_top_picker_request.emit(self)

func _on_color_bottom_picker_request() -> void:
	color_bottom_picker_request.emit(self)

func _on_line_color_name_changed(txt: String) -> void:
	# If there is already another color with this new name, cancel edit
	if get_parent().find_child(txt, false, false):
		var caret = line.caret_column
		line.text = name
		line.caret_column = caret
		return
	
	name_modified.emit(name, txt)
	name = txt
