extends PanelContainer

@onready var hbox_add_entry: HBoxContainer = $VBox/HBox_AddEntry
@onready var line_add_entry: LineEdit = $VBox/HBox_AddEntry/Line_EntryName
@onready var texture_warning: TextureRect = $VBox/HBox_AddEntry/Texture_Warning

@onready var hbox_access_buttons: HBoxContainer = $VBox/Toolbar
@onready var button_add: Button = $VBox/Toolbar/Add
@onready var button_trash: Button = $VBox/Toolbar/Trash
@onready var label_entry_count: Label = $VBox/Toolbar/Label_EntryCount

func _ready() -> void:
	_hide_control_inputs()

#region Events

func _on_add_toggled(toggled_on: bool) -> void:
	_hide_control_inputs(button_add, toggled_on)
	
	if toggled_on:
		hbox_add_entry.show()
		line_add_entry.grab_focus()
		line_add_entry.caret_column = line_add_entry.text.length()

#endregion

#region Utilities

func _hide_control_inputs(selection: Button = null, active: bool = false) -> void:
	hbox_add_entry.hide()
	
	for child in hbox_access_buttons.get_children():
		if child is not Button:
			continue
		
		if child != selection:
			child.button_pressed = false
	
	if selection:
		selection.button_pressed = active

#endregion
