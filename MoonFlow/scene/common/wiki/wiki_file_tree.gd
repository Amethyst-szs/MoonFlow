extends Tree

signal file_selected(path: String)

func _ready() -> void:
	var root := create_item()
	
	var path: String= EngineSettings.get_setting("moonflow/wiki/local_source", "res://docs/")
	_generate_folder(root, path)

func _generate_folder(root: TreeItem, path: String) -> void:
	var subdirs = DirAccess.get_directories_at(path)
	var files = DirAccess.get_files_at(path)
	
	for subdir in subdirs:
		var folder := create_item(root)
		folder.set_text(0, subdir.to_pascal_case())
		folder.set_metadata(0, "__FOLDER__")
		
		_generate_folder(folder, path + subdir + '/')
	
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
