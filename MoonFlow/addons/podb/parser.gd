@tool
extends EditorTranslationParserPlugin

func _parse_file(path: String, msgids: Array[String], msgids_context_plural: Array[Array]) -> void:
	var res: Resource = load(path)
	if not res:
		return

	if res is TranslationBank:
		var item := res as TranslationBank
		
		for str in item.keys:
			msgids_context_plural.append([str, item.context, ""])

func _get_recognized_extensions() -> PackedStringArray:
	return ["tres"]
