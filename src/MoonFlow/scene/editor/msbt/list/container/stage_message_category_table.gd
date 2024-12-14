extends RefCounted
class_name MsbtEntryStageMessageCategoryTable

const context: String = "MSBT_ENTRY_STAGE_MESSAGE_CATEGORY"

var table: Dictionary = {
	"ScenarioName": tr("SCENARIO_NAME", context),
	"Checkpoint": tr("CHECKPOINT", context),
	"LocationNameArea": tr("LOCATION_NAME_AREA", context),
	"Race": tr("RACE", context),
	"Quest": tr("QUEST", context),
}

const ScenarioName_tex: Texture2D = preload("res://asset/nindot/lms/icon/PictureFont_7A.png")
const Checkpoint_tex: Texture2D = preload("res://asset/nindot/lms/icon/PictureFont_42.png")
const LocationNameArea_tex: Texture2D = preload("res://asset/nindot/lms/icon/PictureFont_ForWheel.png")
