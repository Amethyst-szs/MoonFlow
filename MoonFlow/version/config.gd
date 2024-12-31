extends "res://addons/AutoExportVersion/VersionProvider.gd"

func get_version(features: PackedStringArray, is_debug: bool, path: String, flags: int) -> String:
	var version: String = "b%s - %s (%s)" % [
		get_git_commit_count(),
		get_git_branch_name(),
		get_git_commit_hash()
	]
	
	return version

func get_unix(features: PackedStringArray, is_debug: bool, path: String, flags: int) -> String:
	return get_git_commit_unix_time()
