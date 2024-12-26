extends PanelContainer
class_name AlertPopupPart

## Icon to display on the notification popup
var info_texture: Texture = null
## Title of notification message
var info_title: String = ""
## Description of notification displayed until the title
var info_desc: String = ""
## Background color of notification panel
var info_bg_color: Color = Color.BLACK
## Color of progress bar and icon texture
var info_accent_color: Color = Color.YELLOW

## Remaining duration of notification popup
var duration: float = -1.0
## Total length of the notification popup
var duration_length: float = 0.0

@onready var progress_end: ProgressBar = $Progress_End

func setup_notification(tex: Texture, t: String, d: String, c: Color, dur: float) -> void:
	info_texture = tex
	info_title = t
	info_desc = d
	info_bg_color = c.darkened(0.8)
	info_accent_color = c
	duration_length = dur

func notification_appear() -> void:
	$Content/Texture_Icon.texture = info_texture
	$Content/TextContent/Label_Title.text = info_title
	$Content/TextContent/Rich_Desc.text = "[center]" + info_desc
	
	self_modulate = info_bg_color
	$Content/Texture_Icon.modulate = info_accent_color
	
	progress_end.modulate = info_accent_color
	progress_end.max_value = duration_length
	
	duration = duration_length
	
	$Anim.play("appear")

#region Implementation

func _enter_tree() -> void:
	visible = false

func _process(delta: float) -> void:
	if duration == -1.0:
		return
	
	duration -= delta
	progress_end.value = duration
	
	if duration < 0.0:
		$Anim.play("end")
		duration = -1.0

#endregion
