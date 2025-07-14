extends LineEdit

signal text_validated(word: String)
signal validated_text_submitted(word: String)

@export var regex_string: String = "[A-Za-z0-9]"
@export var is_return_if_empty: bool = false

var regex = RegEx.new()

func _ready() -> void:
	regex.compile(regex_string)
	text_changed.connect(_on_text_changed)
	text_submitted.connect(_on_text_submitted)

func _on_text_changed(new_text: String) -> void:
	if is_return_if_empty && new_text.is_empty():
		return
	
	var old_caret_position := caret_column
	
	# Calculate the string filtered by regex
	var word = ""
	for valid_character in regex.search_all(new_text):
		word += valid_character.get_string()
	
	# If the new text and the filtered text are the same, return early
	if new_text == word:
		text_validated.emit(new_text)
		return
	
	set_text(word)
	set_deferred("caret_column", old_caret_position - 1)
	text_validated.emit(word)

func _on_text_submitted(new_text: String) -> void:
	if new_text.is_empty():
		return
	
	# Calculate the string filtered by regex
	var word = ""
	for valid_character in regex.search_all(new_text):
		word += valid_character.get_string()
	
	# If the new text and the filtered text are the same, return early
	if new_text == word:
		validated_text_submitted.emit(new_text)
		return
