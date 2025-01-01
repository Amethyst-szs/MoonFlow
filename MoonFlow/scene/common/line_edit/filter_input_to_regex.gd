extends LineEdit

signal text_validated(word: String)

@export var regex_string: String = "[A-Za-z0-9]"
@export var is_return_if_empty: bool = false

var regex = RegEx.new()

func _ready() -> void:
	regex.compile(regex_string)
	text_changed.connect(_on_text_changed)

func _on_text_changed(new_text: String) -> void:
	if is_return_if_empty && new_text.is_empty():
		return
	
	var old_caret_position = caret_column
	var word = ""
	
	for valid_character in regex.search_all(new_text):
		word += valid_character.get_string()
	
	set_text(word)
	set_deferred("caret_column", old_caret_position)
	text_validated.emit(word)
