extends Popup

@export var autofill_text: bool = false

var input: String = ""

signal submitted(input: String)

@onready var header: Label = %Label_Header
@onready var line_name: LineEdit = %Line_FileName

func _ready() -> void:
	about_to_popup.connect(_on_appear)

func _on_appear() -> void:
	line_name.grab_focus()

func init_data(txt: String) -> void:
	var default_str := txt.substr(0, txt.find("."))
	line_name.clear()
	line_name.placeholder_text = default_str
	
	if autofill_text:
		input = default_str
		line_name.text = default_str
		line_name.caret_column = default_str.length()

func _on_file_name_updated(word: String) -> void:
	input = word

func _on_submit() -> void:
	submitted.emit(input)
	hide()
