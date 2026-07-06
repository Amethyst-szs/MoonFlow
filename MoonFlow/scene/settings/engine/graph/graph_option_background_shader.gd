extends OptionButton

const GraphBackgroundMaterialResType = preload("uid://bd11ygborcgfi")

@export var info: GraphBackgroundMaterialResType

func _ready() -> void:
	for choice in info.theme_dict.keys():
		add_item(tr(choice, info.translation_context))
		
		if choice == info.get_user_shader_choice():
			select(item_count - 1)

func _on_item_selected(index: int) -> void:
	var choice: String = info.theme_dict.keys()[index]
	info.set_user_shader_choice(choice)
