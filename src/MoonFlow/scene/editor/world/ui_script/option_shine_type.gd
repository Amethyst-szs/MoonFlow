extends OptionButton

signal shine_type_id(id: int)

@onready var spin_custom: SpinBox = $"../Spin_ShineTypeCustom"

func set_selection(id: int) -> void:
	if id == -1:
		selected =_on_item_selected(10)
		return
	
	selected = _on_item_selected(id)

func _on_item_selected(index: int) -> int:
	if index >= 11:
		spin_custom.show()
		return index
	
	spin_custom.hide()
	
	if index == 10:
		shine_type_id.emit(-1)
		return 10
	
	shine_type_id.emit(index)
	return index
