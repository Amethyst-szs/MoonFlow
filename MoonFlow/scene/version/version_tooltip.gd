extends VBoxContainer

var txt_name: String
var txt_tag: String
var txt_time: String

@onready var rich: RichTextLabel = $Rich_Info
@onready var footer: Label = $Label_TimestampFooter

func setup_labels(update_name: String, update_tag: String, time: String) -> void:
	txt_name = update_name
	txt_tag = update_tag
	txt_time = time

func _ready() -> void:
	rich.append_text("[center]")
	rich.add_text(txt_name + "\n")
	
	rich.push_font_size(14)
	rich.push_italics()
	rich.push_color(Color.LIGHT_YELLOW)
	rich.add_text(txt_tag)
	
	footer.text = txt_time
