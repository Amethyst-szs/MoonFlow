extends LineEdit

signal text_validated(word: String)

@export var regex_string: String = "[A-Za-z0-9]"
@export var is_return_if_empty: bool = false

func _ready() -> void:
	text_changed.connect(_on_text_changed)

func _on_text_changed(new_text: String) -> void:
	if is_return_if_empty && new_text.is_empty():
		return
	
	var old_caret_position = caret_column
	var word = ""
	
	var regex = RegEx.new()
	regex.compile(regex_string)
	for valid_character in regex.search_all(new_text):
		word += valid_character.get_string()
	set_text(word)
	
	caret_column = old_caret_position
	
	text_validated.emit(word)
