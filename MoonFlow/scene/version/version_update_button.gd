extends Button

static var is_update_checked: bool = false
static var is_update_available: bool = false
static var update_url: String = ""
static var update_asset_download_url: String = ""
static var update_name: String = ""
static var update_tag: String = ""
static var update_unix: int

const setting_group: String = "moonflow/version/update_"
const api_token_path: String = "user://git_api_token"

signal launch_update(download_url: String)

func _ready():
	var request: HTTPRequest = $HTTPRequest
	hide()
	
	# If update already checked, skip HTTP request
	if is_update_checked:
		request.queue_free()
		if is_update_available:
			_appear_update_button()
		
		return
	
	# Get HTTP request url and api headers
	var url := _get_api_request_url()
	var header: PackedStringArray = ProjectSettings.get_setting(
		setting_group + "domain_api_headers", []
	)
	
	# If the user has an local api token, include in headers
	if FileAccess.file_exists(api_token_path):
		var token_header := FileAccess.get_file_as_string(api_token_path)
		header.append(token_header)
	
	# Request data
	request.request(url, header, HTTPClient.METHOD_GET)

#region API Response

func _on_request_completed(_r: int, response_code: int, _h: PackedStringArray, body_b: PackedByteArray) -> void:
	is_update_checked = true
	
	if response_code != 200:
		print_v_error("Update check failed (%s)" % response_code)
		return
	
	var body: Dictionary = JSON.parse_string(body_b.get_string_from_utf8())
	if body == null:
		print_v_error("Failed to parse update API JSON")
		return
	
	# Get information about the release
	update_url = body.get("html_url")
	update_name = body.get("name")
	update_tag = body.get("tag_name")
	
	if update_url == null || update_name == null || update_tag == null:
		print_v_error("Failed to read values from update API")
		return
	
	var unix_str = body.get("created_at")
	if unix_str == null:
		print_v_error("Failed to get update unix string")
		return
	
	# Get the remote unix time and comapre it to local
	update_unix = int(Time.get_unix_time_from_datetime_string(unix_str))
	
	if !OS.get_cmdline_user_args().has("--ignore_update_timestamp"):
		if GitInfo.commit_time_unix > update_unix:
			print_v("Most recent release is behind current build, ignoring updater")
			return
		
		if GitInfo.commit_time_unix == update_unix:
			print_v("No updates available")
			return
	
	# Attempt to find the correct asset package for local machine
	var target_asset_name: String = _get_target_asset_name()
	var target_arch_name: String = _get_target_asset_name()
	
	if target_asset_name != "":
		var asset_list: Array = body.get("assets")
		
		for asset in asset_list:
			if !asset.has("name"): continue
			
			var asset_name: String = asset.get("name") as String
			if !asset_name.contains(target_asset_name): continue
			if !asset_name.contains(target_arch_name): continue
			
			update_asset_download_url = asset.get("browser_download_url")
	
	print_v("Update available: " + update_tag)
	_appear_update_button()

func _appear_update_button() -> void:
	is_update_available = true
	
	var timezone: int = Time.get_time_zone_from_system().bias * 60
	var remote_unix_local: int = update_unix + timezone
	var timestr := Time.get_datetime_string_from_unix_time(remote_unix_local, true)
	
	tooltip_text = "%s;%s;%s" % [
		update_name,
		update_tag,
		timestr
	]
	
	pressed.connect(_on_updater_pressed.bind(update_url))
	show()

func _on_updater_pressed(url: String) -> void:
	if update_asset_download_url == "":
		OS.shell_open(url)
		return
	
	print_v("Launching update utility: " + update_asset_download_url)
	launch_update.emit(update_asset_download_url)

func _get_api_request_url() -> String:
	var domain: String = ProjectSettings.get_setting(setting_group + "domain_api", "")
	var user: String = ProjectSettings.get_setting(setting_group + "user", "")
	var repo: String = ProjectSettings.get_setting(setting_group + "repo", "")
	
	var path := "%s/%s/%s/releases/latest" % [domain, user, repo]
	print_v("Checking for update at: " + path)
	return path

#endregion

#region Custom Tooltip

const tooltip_scene := preload("res://scene/version/version_tooltip.tscn")

func _make_custom_tooltip(for_text: String) -> Control:
	var txt := for_text.split(';', false)
	if txt.size() != 3:
		push_error("Invalid tooltip text!")
	
	var scene = tooltip_scene.instantiate()
	scene.setup_labels(txt[0], txt[1], txt[2])
	return scene

#endregion

#region Print Utility

func print_v(msg: String) -> void:
	print_rich("[right][color=GRAY][i] |> " + msg)

func print_v_error(msg: String) -> void:
	print_rich("[right][color=INDIAN_RED][i] |> " + msg)

#endregion

#region Utility

func _get_target_asset_name() -> String:
	var server := DisplayServer.get_name()
	
	if server == "Windows": return "win"
	if server == "X11" || server == "Wayland": return "linux"
	
	return ""

func _get_target_asset_architecture() -> String:
	return Engine.get_architecture_name()

#endregion
