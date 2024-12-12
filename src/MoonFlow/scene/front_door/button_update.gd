extends Button

const url_raw: String = "https://raw.githubusercontent.com"
const url_end: String = "https://github.com"

const user: String = "Amethyst-szs"
const repo: String = "MoonFlow"
const branch: String = "main"

var check_path: String = "%s/%s/%s/%s/version" % [url_raw, user, repo, branch]
var release_path: String = "%s/%s/%s/releases/latest" % [url_end, user, repo]
const check_down_path: String = "user://remote_version"

var current: String = preload("res://version/data.gd").VERSION

func _ready():
	hide()
	
	var request: HTTPRequest = $HTTPRequest
	request.download_file = check_down_path
	request.request(check_path)

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
	OS.shell_open(release_path)
