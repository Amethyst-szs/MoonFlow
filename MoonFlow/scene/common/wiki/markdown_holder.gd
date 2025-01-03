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
