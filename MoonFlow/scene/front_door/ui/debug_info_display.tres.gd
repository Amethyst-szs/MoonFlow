extends RichTextLabel

func _ready() -> void:
	append_text("[right]")
	
	_append_git_info()
	_append_compile_info()
	_append_engine_info()
	
	meta_clicked.connect(_on_meta_pressed)

func _append_git_info() -> void:
	# Calculate commit date
	var timezone: int = Time.get_time_zone_from_system().bias * 60
	var unix_l: int = GitInfo.commit_time_unix + timezone
	var time: String = Time.get_date_string_from_unix_time(unix_l)
	
	# Setup git info
	var out: String = ""
	out += time + '\n'
	out += GitInfo.author + '\n'
	out += _create_metahint(GitInfo.version_name, GitInfo.commit_hash) + '\n'
	
	append_text("[color=MISTY_ROSE]%s[/color]" % out)

func _append_compile_info() -> void:
	var n: String = ProjectSettings.get_setting("application/config/name", "MoonFlow")
	var repo: String = ProjectSettings.get_setting("moonflow/wiki/home_repository", "https://github.com/Amethyst-szs/MoonFlow")
	
	var content := "%s (%s)" % [n, DisplayServer.get_name()]
	if OS.is_debug_build(): content += " DEBUG"
	
	var out := "[color=PALE_GREEN][i]%s[/i][/color]\n" % content
	append_text(_create_metahint(out, repo))

func _append_engine_info() -> void:
	var engine: String = Engine.get_version_info()["string"]
	var arch := Engine.get_architecture_name()
	
	var out := "[i]%s (%s)[/i]" % [engine, arch]
	out = _create_metahint(out, "https://godotengine.org/")
	
	append_text("[color=LIGHT_CYAN]%s[/color]" % out)

func _on_meta_pressed(item: Variant) -> void:
	if item is not String: return
	var txt: String = item
	
	if txt.begins_with("https://"):
		print("Opening %s in default browser" % txt)
		OS.shell_open(txt)
		return
	
	print("Copied %s to clipboard!" % txt)
	DisplayServer.clipboard_set(txt)

#region Utility

func _create_meta(txt: String, url: String) -> String:
	return "[url=%s]%s[/url]" % [url, txt]

func _create_hint(txt: String, hint: String) -> String:
	return "[hint=%s]%s[/hint]" % [hint, txt]

func _create_metahint(txt: String, metahint: String) -> String:
	return "[url=%s][hint=%s]%s[/hint][/url]" % [metahint, metahint, txt]

#endregion
