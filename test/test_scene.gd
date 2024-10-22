extends Node

@onready var msbt := MsbtResource.FromFilePath("res://test/AnimalChaseExStage.msbt")
@onready var szs := SarcResource.FromFilePath("res://test/TalkNpc.szs")

# Called when the node enters the scene tree for the first time.
func _ready():
	var byml: BymlResource = szs.GetFileByml("GiveShine_Npc.byml")
	
