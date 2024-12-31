extends Button

const setting_group: String = "moonflow/version/update_"
const check_down_path: String = "user://remote_version"

var current: String = preload("res://version/data.gd").VERSION

func _ready():
	hide()
	
	var request: HTTPRequest = $HTTPRequest
	request.download_file = check_down_path
	request.request(_get_http_request_url())

func _on_request_completed(_r: int, response_code: int, _h: PackedStringArray, _b: PackedByteArray) -> void:
	if response_code != 200:
		print(" -> Update check failed (%s) <- " % response_code)
		return
	
	if !FileAccess.file_exists(check_down_path):
		push_error("Update check successed but output file is missing!")
		return
	
	var remote_ver: String = FileAccess.get_file_as_string(check_down_path)
	if remote_ver.is_empty():
		return
	
	if remote_ver != current:
		print(" -> Update available - " + remote_ver + " <- ")
		tooltip_text = tr(tooltip_text) + "\nNew: %s" % remote_ver
		show()

func _pressed() -> void:
	OS.shell_open(_get_download_url())

func _get_http_request_url() -> String:
	var domain: String = ProjectSettings.get_setting(setting_group + "domain_raw", "")
	var user: String = ProjectSettings.get_setting(setting_group + "user", "")
	var repo: String = ProjectSettings.get_setting(setting_group + "repo", "")
	var branch: String = ProjectSettings.get_setting(setting_group + "channel", "")
	
	var path := "%s/%s/%s/%s/version" % [domain, user, repo, branch]
	print("Checking for update at: " + path)
	
	return path

func _get_download_url() -> String:
	var domain: String = ProjectSettings.get_setting(setting_group + "domain", "")
	var user: String = ProjectSettings.get_setting(setting_group + "user", "")
	var repo: String = ProjectSettings.get_setting(setting_group + "repo", "")
	
	var path := "%s/%s/%s/releases/latest" % [domain, user, repo]
	print("Opening browser to: " + path)
	
	return path
