extends PanelContainer

@onready var separator: VSeparator = $"../VSep"

var tween: Tween = null

func _ready() -> void:
	separator.hide()
	hide()

func set_visibility() -> void:
	if tween != null: return
	
	if !visible: appear()
	else: close()

func appear() -> void:
	separator.modulate = Color.TRANSPARENT
	modulate = Color.TRANSPARENT
	
	separator.show()
	show()
	
	tween = create_tween().set_trans(Tween.TRANS_SINE).set_parallel()
	tween.tween_property(self, "modulate", Color.WHITE, 0.25)
	tween.tween_property(separator, "modulate", Color.WHITE, 0.25)
	await tween.finished
	
	tween = null

func close() -> void:
	separator.show()
	show()
	
	tween = create_tween().set_trans(Tween.TRANS_SINE).set_parallel()
	tween.tween_property(self, "modulate", Color.TRANSPARENT, 0.15)
	tween.tween_property(separator, "modulate", Color.TRANSPARENT, 0.15)
	await tween.finished
	
	hide()
	tween = null
