extends "res://addons/AutoExportVersion/VersionProvider.gd"

func get_version(_features: PackedStringArray, _is_debug: bool, _path: String, _flags: int) -> String:
	var version: String = "b%s - %s (%s)" % [
		get_git_commit_count(),
		get_git_branch_name(),
		get_git_commit_hash()
	]
	
	return version

func get_unix(_features: PackedStringArray, _is_debug: bool, _path: String, _flags: int) -> String:
	return get_git_commit_unix_time()
