extends HBoxContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	$Panel_Label/Label.text = name
	$Panel_Path/Label_Set.text = ""

func set_path(path: String) -> void:
	$Panel_Path/Label_Set.text = path
	$Panel_Path/Label_Set.show()
	$Panel_Path/Label_Unset.hide()

func set_no_path() -> void:
	$Panel_Path/Label_Set.hide()
	$Panel_Path/Label_Unset.show()
