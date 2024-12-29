extends HBoxContainer

signal color_picker_request(source: Node)
signal name_modified(old_name: String, new_name: String)

@onready var button: Button = $Panel_Color/Layout/Button_OpenPicker
@onready var line: LineEdit = $Panel_Color/Layout/Line_ColorName

@onready var preview_b: RichTextLabel = $Panel_PreviewBlack/Preview
@onready var preview_w: RichTextLabel = $Panel_PreviewWhite/Preview

func setup(n: String, color: Color) -> void:
	name = n
	line.text = n
	
	set_color(color)

func set_color(color: Color) -> void:
	button.self_modulate = color
	preview_b.self_modulate = color
	preview_w.self_modulate = color

func _on_color_picker_request() -> void:
	color_picker_request.emit(self)

func _on_line_color_name_changed(txt: String) -> void:
	# If there is already another color with this new name, cancel edit
	if get_parent().find_child(txt, false, false):
		var caret = line.caret_column
		line.text = name
		line.caret_column = caret
		return
	
	name_modified.emit(name, txt)
	name = txt
