@tool

extends Object
class_name EditorPluginGitInterface

static func git_project_version_name() -> String:
	var count := git_commit_count_main_branch()
	var ahead := git_commit_ahead_count()
	var branch := git_branch_name()
	var hash := git_commit_hash_short()
	
	return "%s b%d.%d (%s)" % [branch, count, ahead, hash]

#region Branch Info

static func git_branch_name() -> String:
	return _exe_rev_parse(["--abbrev-ref", "HEAD"])

#endregion

#region Commit Info

static func git_commit_subject() -> String:
	return _exe_show(["--no-patch", "--format=%s"])

static func git_commit_author_name() -> String:
	return _exe_show(["--no-patch", "--format=%cn"])

static func git_commit_hash() -> String:
	return _exe_rev_parse(["HEAD"])

static func git_commit_hash_short() -> String:
	return _exe_rev_parse(["HEAD"]).left(8)

static func git_commit_count() -> int:
	return int(_exe_rev_list(["--count", "HEAD"]))

static func git_commit_count_main_branch() -> int:
	var branch: String = ProjectSettings.get_setting("moonflow/version/main_branch", "stable")
	return int(_exe_rev_list(["--count", branch]))

static func git_commit_ahead_count() -> int:
	return int(_exe_rev_list(["--count", "HEAD", "^" + "stable"]))

static func git_commit_unix_time() -> int:
	return int(_exe_show(["--no-patch", "--format=%ct"]))

#endregion

#region Author Info

static func git_author_name() -> String:
	return _exe_show(["--no-patch", "--format=%an"])

#endregion

#region OS Excution Util

static func _exe_rev_parse(args: PackedStringArray) -> String:
	var output: Array = []
	args.insert(0, "rev-parse")
	OS.execute("git", args, output)
	
	return _exe_result(output)

static func _exe_rev_list(args: PackedStringArray) -> String:
	var output: Array = []
	args.insert(0, "rev-list")
	OS.execute("git", args, output)
	
	return _exe_result(output)

static func _exe_show(args: PackedStringArray) -> String:
	var output: Array = []
	args.insert(0, "show")
	OS.execute("git", args, output)
	
	return _exe_result(output)

static func _exe_result(result: Array) -> String:
	if result.is_empty() || result[0] is not String || result[0].is_empty():
		push_error("Git command failed to recieve valid result!")
		return ""
	
	return result[0].trim_suffix("\n")

#endregion

#region Debug

static func log_all_info() -> void:
	var instance := new()
	var methods := instance.get_method_list()
	
	print_rich("\n[b]Git Repository:")
	
	for method in methods:
		if !method.name.begins_with("git_"):
			continue
		
		print("%s: %s" % [method.name, instance.call(method.name)])
	
	instance.free()

#endregion
