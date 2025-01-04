@tool
extends Tree

signal file_selected(path: String)

func _ready() -> void:
	var root := create_item()
	var root_path: String
	
	if Engine.is_editor_hint():
		root_path = ProjectSettings.get_setting("moonflow/wiki/local_source")
	else:
		root_path = EngineSettings.get_setting("moonflow/wiki/local_source", "res://docs/")
	
	_generate_folder(root, root_path)

func _generate_folder(root: TreeItem, path: String) -> void:
	var subdirs = DirAccess.get_directories_at(path)
	var files = DirAccess.get_files_at(path)
	
	for subdir in subdirs:
		var target: String = path + subdir + '/'
		var f_list := DirAccess.get_files_at(target)
		
		var f_count: int = 0
		var d_count := DirAccess.get_directories_at(target).size()
		
		for i in range(f_list.size()):
			if f_list[i].ends_with(".md"): f_count += 1
		
		if f_count == 0 && d_count == 0:
			continue
		
		var folder := create_item(root)
		folder.set_text(0, subdir.to_pascal_case())
		folder.set_metadata(0, "__FOLDER__")
		folder.set_custom_font_size(0, 18)
		
		_generate_folder(folder, target)
	
	for file in files:
		var item := create_item(root)
		item.set_metadata(0, path + file)
		
		var txt := file.trim_suffix(".md").to_pascal_case()
		item.set_text(0, txt)

func _on_item_selected():
	var sel := get_selected()
	var meta: String = sel.get_metadata(0)
	
	if meta == "__FOLDER__":
		allow_reselect = false
		sel.collapsed = !sel.collapsed
		allow_reselect = true
		return
	
	if !FileAccess.file_exists(meta):
		push_warning("Requested documentation file doesn't exist!")
		return
	
	file_selected.emit(meta)

func set_selection(path: String) -> void:
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
