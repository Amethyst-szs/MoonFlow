@tool
extends EditorTranslationParserPlugin

func _parse_file(path: String) -> Array[PackedStringArray]:
	var ret: Array[PackedStringArray] = []
	
	var res: Resource = load(path)
	if not res:
		return ret

	if res is TranslationBank:
		_parse_type_translation_bank(res, ret)
	
	if res is ContributorList:
		_parse_type_contributor_list(res, ret)
	
	return ret

func _parse_type_translation_bank(res: TranslationBank, ret: Array[PackedStringArray]) -> void:
	for str in res.keys:
		ret.append(PackedStringArray([str, res.context, ""]))

func _parse_type_contributor_list(res: ContributorList, ret: Array[PackedStringArray]) -> void:
	ret.append(PackedStringArray([res.list_name]))

func _get_recognized_extensions() -> PackedStringArray:
	return ["tres"]
