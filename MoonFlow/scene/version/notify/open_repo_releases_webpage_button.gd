extends Button

func _pressed() -> void:
	var path: String = ProjectSettings.get_setting("moonflow/version/repo_releases_url", "https://github.com/Amethyst-szs/MoonFlow/releases")
	OS.shell_open(path)
