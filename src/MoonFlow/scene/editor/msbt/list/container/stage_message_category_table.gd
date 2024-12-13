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
