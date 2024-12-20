extends EditorImportPlugin
class_name MfgraphImportPlugin

func _import(source_file, save_path, options, platform_variants, gen_files):
	var res = MfgraphResource.new()
	res.call("InitResource", source_file)
	return ResourceSaver.save(res, "%s.%s" % [save_path, _get_save_extension()])

func _get_importer_name():
	return "moonflow.mfgraph"

func _get_visible_name():
	return "mfgraph"

func _get_recognized_extensions():
	return ["mfgraph"]

func _get_save_extension():
	return "res"

func _get_resource_type():
	return "Resource"

func _get_preset_count():
	return 1

func _get_preset_name(preset_index):
	return "mfgraph"

func _get_priority():
	return 1

func _get_import_order():
	return 1

func _get_import_options(path, preset_index):
	return []

func _get_option_visibility(path, option_name, options):
	return true
