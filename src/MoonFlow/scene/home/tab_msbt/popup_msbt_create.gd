extends Popup

var titles: Array[String] = [
	tr("HOME_MSBT_POPUP_TITLE_DUPLICATE"),
	tr("HOME_MSBT_POPUP_TITLE_NEW"),
]

@export_enum("Duplicate:0", "New:1") var title_selection: int = 0

var archive_id: int = 0:
	set(value):
		archive_id = value
		option_arc.select(archive_id)

var file_name: String = ""

signal submitted(archive: String, file_name: String)

@onready var header: Label = %Label_Header
@onready var option_arc: OptionButton = %Option_Archive
@onready var line_name: LineEdit = %Line_FileName

func _ready() -> void:
	header.text = titles[title_selection]

func init_data(arc: String, source_name: String) -> void:
	match(arc):
		"SystemMessage.szs": archive_id = 0
		"StageMessage.szs": archive_id = 1
		"LayoutMessage.szs": archive_id = 2
	
	line_name.clear()
	line_name.placeholder_text = source_name.substr(0, source_name.find(".msbt"))

func _on_file_name_updated(word: String) -> void:
	file_name = word

func _on_archive_selected(index: int) -> void:
	archive_id = index

func _on_submit() -> void:
	var archive: String = ""
	match(archive_id):
		0: archive = "SystemMessage.szs"
		1: archive = "StageMessage.szs"
		2: archive = "LayoutMessage.szs"
	
	submitted.emit(archive, file_name)
	hide()