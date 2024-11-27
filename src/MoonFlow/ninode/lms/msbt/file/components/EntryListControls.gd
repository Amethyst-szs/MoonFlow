extends PanelContainer

@onready var entry_button_list_scroll: SmoothScrollContainer = $"../Scroll"
@onready var entry_button_list: VBoxContainer = $"../Scroll/List"

@onready var line_search: LineEdit = $VBox/Line_Search

@onready var hbox_add_entry: HBoxContainer = $VBox/HBox_AddEntry
@onready var line_add_entry: LineEdit = $VBox/HBox_AddEntry/Line_EntryName
@onready var texture_warning: TextureRect = $VBox/HBox_AddEntry/Texture_Warning

@onready var hbox_access_buttons: HBoxContainer = $VBox/HBox
@onready var button_add: Button = $VBox/HBox/Add
@onready var button_search: Button = $VBox/HBox/Search
@onready var button_trash: Button = $VBox/HBox/Trash
@onready var label_entry_count: Label = $VBox/HBox/EntryCount

func _ready() -> void:
	_hide_control_inputs()
	_on_msbt_editor_add_new_entry_validity(false)

#region Events

func _on_msbt_editor_entry_count_updated(total: int, search_matching: int) -> void:
	if total == search_matching:
		label_entry_count.text = str(total) + " Entries"
		return
	
	label_entry_count.text = "%d/%d Entries" % [search_matching, total]

func _on_msbt_editor_add_new_entry_validity(isValid: bool) -> void:
	texture_warning.visible = !isValid

func _on_button_add_entry_pressed():
	line_add_entry.grab_focus()
	line_add_entry.text_submitted.emit(line_add_entry.text)

func _on_line_new_entry_text_submitted(new_text: String) -> void:
	await Engine.get_main_loop().process_frame
	
	var new_child: Control = entry_button_list.get_node(new_text)
	if !new_child:
		return
	
	entry_button_list_scroll.scroll_y_to(-new_child.position.y)

func _on_add_toggled(toggled_on: bool) -> void:
	_hide_control_inputs(button_add, toggled_on)
	
	if toggled_on:
		hbox_add_entry.show()
		line_add_entry.grab_focus()

func _on_search_toggled(toggled_on: bool) -> void:
	_hide_control_inputs(button_search, toggled_on)
	
	if toggled_on:
		line_search.show()
		line_search.grab_focus()

func _on_trash_pressed() -> void:
	_hide_control_inputs(button_trash)

#endregion

#region Utilities

func _hide_control_inputs(selection: Button = null, active: bool = false) -> void:
	line_search.hide()
	hbox_add_entry.hide()
	
	for child in hbox_access_buttons.get_children():
		if child is not Button:
			continue
		
		if child != selection:
			child.button_pressed = false
	
	if selection:
		selection.button_pressed = active

#endregion
