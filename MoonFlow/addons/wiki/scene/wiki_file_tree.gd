@tool
extends Tree

const table := preload("res://addons/wiki/plugin/table.json").data

signal file_selected(path: String)

func _ready() -> void:
	var root := create_item()
	_generate_folder(root, "res://docs/", table)

func _generate_folder(root: TreeItem, path: String, data: Dictionary) -> void:
	# Attempt to add file list
	if data.has("__"):
		for file in data["__"]:
			var item := create_item(root)
			item.set_metadata(0, path + file)
			
			var txt: String = file.trim_suffix(".md").capitalize()
			item.set_text(0, txt)
	
	# Add all subdirectories
	for dir in data.keys():
		if dir == "__": continue
		
		var target: String = path + dir + '/'
		
		var folder := create_item(root)
		folder.set_text(0, dir.capitalize())
		folder.set_metadata(0, "__FOLDER__")
		folder.set_custom_font_size(0, 18)
		folder.collapsed = true
		
		_generate_folder(folder, target, data[dir])

func _on_item_selected():
	var sel := get_selected()
	var meta: String = sel.get_metadata(0)
	
	if meta == "__FOLDER__":
		allow_reselect = false
		sel.collapsed = !sel.collapsed
		allow_reselect = true
		return
	
	file_selected.emit(meta)

func set_selection(path: String) -> void:
	while path.contains("../"):
		var end := path.find("../")
		var start := end - 2
		while path[start] != '/':
			start -= 1
		
		path = path.left(start) + '/' + path.right(-end - "../".length())
		continue
	
	var item: TreeItem = get_root()
	while item != null:
		if item.get_metadata(0) == path:
			break
		
		item = item.get_next_in_tree()
	
	if item == null:
		push_warning("Could not resolve document target!")
		return
	
	set_selected(item, 0)
	queue_redraw()
