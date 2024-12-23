extends VBoxContainer

func setup(info: ContributorList) -> void:
	var header: Label = $HBox_Head/Label
	var icon: TextureRect = $HBox_Head/Icon
	
	header.text = info.list_name
	header.self_modulate = info.list_color
	icon.texture = info.list_icon
	
	if info.list_icon == null:
		icon.hide()
	
	var entry_list: GridContainer = $Entries
	entry_list.columns = min(info.list.size(), 3)
	
	for c in info.list:
		var label := RichTextLabel.new()
		label.fit_content = true
		label.bbcode_enabled = true
		label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		label.size_flags_vertical = Control.SIZE_EXPAND_FILL
		
		label.push_meta(c.url, RichTextLabel.META_UNDERLINE_ON_HOVER)
		label.append_text("[center]" + c.name)
		
		if c is ContributorModule:
			label.push_font_size(12)
			label.push_color(Color.LIGHT_GRAY)
			label.add_text(" (%s)" % c.author)
		
		label.meta_clicked.connect(_on_meta_clicked)
		
		entry_list.add_child(label)

func _on_meta_clicked(meta: String) -> void:
	OS.shell_open(meta)
