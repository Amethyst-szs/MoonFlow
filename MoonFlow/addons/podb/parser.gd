@tool
extends EditorTranslationParserPlugin

const GraphBackgroundMaterialResType = preload("uid://bd11ygborcgfi")

func _parse_file(path: String) -> Array[PackedStringArray]:
	var ret: Array[PackedStringArray] = []
	
	var res: Resource = load(path)
	if not res:
		return ret

	if res is TranslationBank:
		_parse_type_translation_bank(res, ret)
	
	if res is ContributorList:
		_parse_type_contributor_list(res, ret)
	
	if res is GraphBackgroundMaterialResType:
		for key in res.theme_dict.keys():
			ret.append(PackedStringArray([key, res.translation_context, ""]))
	
	return ret

func _parse_type_translation_bank(res: TranslationBank, ret: Array[PackedStringArray]) -> void:
	for str in res.keys:
		ret.append(PackedStringArray([str, res.context, ""]))
		
		if res.create_multiple_contexts:
			for ctx in res.additional_contexts:
				ret.append(PackedStringArray([str, ctx, ""]))

func _parse_type_contributor_list(res: ContributorList, ret: Array[PackedStringArray]) -> void:
	ret.append(PackedStringArray([res.list_name]))

func _get_recognized_extensions() -> PackedStringArray:
	return ["tres"]
