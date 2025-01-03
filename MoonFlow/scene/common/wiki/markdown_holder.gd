extends MarkdownLabel

var resource_path: String:
	get: return resource_path
	set(value):
		resource_path = value
		resource_changed.emit(value, local_path)

var local_path: String:
	get: return resource_path.trim_prefix(local_path_base)

var local_path_base: String:
	get: return EngineSettings.get_setting("moonflow/wiki/local_source", "")

static var remote_prefix: String = EngineSettings.get_setting(
	"moonflow/wiki/remote_source", ""
)

signal resource_changed(path: String, local_path: String)

func setup_app(path: String) -> void:
	resource_path = path
	
	if !FileAccess.file_exists(path):
		push_warning("Documentation file doesn't exist! ", path)
		return
	
	display_file(path)
	
	if !unhandled_link_clicked.is_connected(_on_meta):
		unhandled_link_clicked.connect(_on_meta)

func _on_meta(meta: Variant) -> void:
	if meta is String: _on_link(meta as String)

func _on_link(url: String) -> void:
	# Format incoming url string
	url = url.trim_prefix(remote_prefix).replace('<', '').replace('>', '')
	var base: String = url.substr(0, url.find('#'))
	var target: String = url.substr(url.find('#'))
	
	# Handle header jumps
	if base.is_empty() and !target.is_empty():
		_try_scroll_to_header(target)
		return
	
	# Handle remote url jumps
	if base.begins_with("http") and target.is_empty():
		OS.shell_open(base)
		return
	
	# Handle internal documentation file jumps
	var file: String = base
	if !file.ends_with(".md"): file += ".md"
	
	if FileAccess.file_exists(local_path_base + file):
		resource_path = local_path_base + file
		display_file(resource_path)
		scroll_to_line(0)
		
		# Attempt a header jump on the newly opened file
		if !target.is_empty():
			_try_scroll_to_header(target)

func _try_scroll_to_header(target: String) -> void:
	target = target.replace(' ', '-').to_lower()
	if _header_anchor_paragraph.has(target):
		scroll_to_paragraph(_header_anchor_paragraph[target])

#region Overrides

var blockquote_depth: int = 0

const hseparator_keys: PackedStringArray = ["[i]_[/i]", "---", "[i]*[/i]"]

func _preprocess_line(line: String) -> String:
	return line

func _process_custom_syntax(line: String) -> String:
	# Process horizontal line breaks
	for key in hseparator_keys:
		if line.begins_with(key):
			line = "[font_size=31]⸺⸺⸺⸺⸺⸺⸺⸺⸺⸺⸺⸺⸺⸺[/font_size]"
			break
	
	# Process block quotes
	if line.is_empty():
		blockquote_depth = 0
	
	var local_blockquote_depth: int = 0
	var remove_count: int = 0
	for c in line:
		if c == '>':
			local_blockquote_depth += 1
			remove_count += 1
			continue
		
		if c == ' ':
			remove_count += 1
			continue
		
		break
	
	if local_blockquote_depth > 0:
		line = "｜ " + line.substr(remove_count)
		blockquote_depth = local_blockquote_depth
	
	for i in range(blockquote_depth):
		line = "	" + line
	
	return line

#endregion
