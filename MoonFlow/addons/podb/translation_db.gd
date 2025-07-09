class_name TranslationBank
extends Resource

@export var context: String = ""
@export var keys: Array[String] = []

@export_group("Additional Options")
@export var create_multiple_contexts: bool = false
@export var additional_contexts: Array[String] = []
