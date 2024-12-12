extends Node

# Fully aware this is a really weird way to implement this
# I want these strings from AsyncDisplay.cs to show up in the autogen POT
# but they don't without this file. I'm sure I'll add a better way of
# implementing this later lol

var list: Array[String] = [
	tr("ASYNC_DISPLAY_SUCCESS"),
	
	tr("ASYNC_TASK_DISPLAY_FileRead"),
	tr("ASYNC_TASK_DISPLAY_FileWrite"),
	tr("ASYNC_TASK_DISPLAY_FTP"),
	
	tr("ASYNC_TASK_DISPLAY_SaveMsbtArchives"),
	tr("ASYNC_TASK_DISPLAY_SaveWorldArchives"),
]
