@tool
extends EditorTranslationParserPlugin

func _parse_file(path: String, msgids: Array[String], msgids_context_plural: Array[Array]) -> void:
	var res: Resource = load(path)
	if not res:
		return

	if res is TranslationBank:
		_parse_type_translation_bank(res, msgids_context_plural)
	
	if res is ContributorList:
		_parse_type_contributor_list(res, msgids)

func _parse_type_translation_bank(res: TranslationBank, msgids_context_plural: Array[Array]) -> void:
	for str in res.keys:
		msgids_context_plural.append([str, res.context, ""])

func _parse_type_contributor_list(res: ContributorList, msgids: Array[String]) -> void:
	msgids.append(res.list_name)

func _get_recognized_extensions() -> PackedStringArray:
	return ["tres"]
