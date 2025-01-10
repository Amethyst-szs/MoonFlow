extends Button

const setting_group: String = "moonflow/version/update_"
const api_token_path: String = "user://git_api_token"

func _ready():
	hide()
	
	var request: HTTPRequest = $HTTPRequest
	
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

func _on_request_completed(_r: int, response_code: int, _h: PackedStringArray, body_b: PackedByteArray) -> void:
	if response_code != 200:
		print_v_error("Update check failed (%s)" % response_code)
		return
	
	var body: Dictionary = JSON.parse_string(body_b.get_string_from_utf8())
	if body == null:
		print_v_error("Failed to parse update API JSON")
		return
	
	# Get information about the release
	var update_url = body.get("html_url")
	var update_name = body.get("name")
	var update_tag = body.get("tag_name")
	
	if update_url == null || update_name == null || update_tag == null:
		print_v_error("Failed to read values from update API")
		return
	
	var unix_str = body.get("created_at")
	if unix_str == null:
		print_v_error("Failed to get update unix string")
		return
	
	# Get the remote unix time and comapre it to local
	var remote_unix := int(Time.get_unix_time_from_datetime_string(unix_str))
	if GitInfo.commit_time_unix > remote_unix:
		print_v("Most recent release is behind current build, ignoring updater")
		return
	
	if GitInfo.commit_time_unix == remote_unix:
		print_v("No updates available")
		return
	
	# New update available, setup button
	print_v("Update available: " + update_tag)
	
	var timezone: int = Time.get_time_zone_from_system().bias * 60
	var remote_unix_local: int = remote_unix + timezone
	var timestr := Time.get_datetime_string_from_unix_time(remote_unix_local, true)
	
	tooltip_text = "%s\n%s\n%s\n%s" % [tr(tooltip_text), update_name, update_tag, timestr]
	
	pressed.connect(_on_updater_pressed.bind(update_url))
	show()

func _on_updater_pressed(url: String) -> void:
	OS.shell_open(url)

func _get_api_request_url() -> String:
	var domain: String = ProjectSettings.get_setting(setting_group + "domain_api", "")
	var user: String = ProjectSettings.get_setting(setting_group + "user", "")
	var repo: String = ProjectSettings.get_setting(setting_group + "repo", "")
	
	var path := "%s/%s/%s/releases/latest" % [domain, user, repo]
	print_v("Checking for update at: " + path)
	return path

func print_v(msg: String) -> void:
	print_rich("[right][color=GRAY][i] |> " + msg)

func print_v_error(msg: String) -> void:
	print_rich("[right][color=INDIAN_RED][i] |> " + msg)
