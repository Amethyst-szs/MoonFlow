using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Project;
using MoonFlow.Async;
using MoonFlow.Scene.Main;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbp/color/msbp_color_editor.tscn")]
[Icon("res://asset/nindot/lms/icon/System_Color_ForWheel.png")]
public partial class MsbpColorEditor : AppScene
{
	[Export]
	private PackedScene ElementScene;
	[Export]
	private VBoxContainer ElementHolder;
	[Export]
	private Popup ColorPickerHolder;
	[Export]
	private ColorPicker ColorPicker;

	private Node ColorPickerTarget = null;

	protected override void AppInit()
	{
		// Get list of colors
		var msbp = ProjectManager.GetMSBP();
		var labels = msbp.Color_GetLabelList();

		// Create color table
		foreach (var label in labels)
		{
			var element = ElementScene.Instantiate<HBoxContainer>();
			ElementHolder.AddChild(element);

			element.Call("setup", label, msbp.Color_Get(label).ToGodotColor());

			element.Connect("color_picker_request", Callable.From(
				new Action<Node>(OnColorPickerRequest)
			));

			element.Connect("name_modified", Callable.From(
				new Action<string, string>(OnColorNameChanged)
			));
		}

		// Setup signals with header
		var header = ProjectManager.SceneRoot.NodeHeader;
		header.Connect(Header.SignalName.ButtonSave, Callable.From(new Action<bool>(SaveFileInternal)));
	}

	public override string GetUniqueIdentifier(string input)
	{
		return "MSBP_COLOR_" + input;
	}

	#region Save

	private async void SaveFileInternal(bool isRequireFocus) { await AppSaveContent(isRequireFocus); }
	protected override void TaskWriteAppSaveContent(AsyncDisplay display)
	{
		// Write ProjectData archive to disk
		display.UpdateProgress(0, 1);
		ProjectManager.GetMSBP().WriteArchive();

		// Reset flags
		display.UpdateProgress(1, 1);
		IsModified = false;
	}

	#endregion

	#region Signals

	private void OnColorPickerRequest(Node source)
	{
		// Assign picker target
		ColorPickerTarget = source;

		// Set picker's current color
		var msbp = ProjectManager.GetMSBP();
		ColorPicker.Color = msbp.Color_Get(source.Name).ToGodotColor();

		// Spawn picker
		ColorPickerHolder.Position = (Vector2I)GetGlobalMousePosition();
		ColorPickerHolder.Popup();
	}

	private void OnColorPickerColorChanged(Color c)
	{
		var name = ColorPickerTarget.Name;

		var msbp = ProjectManager.GetMSBP();
		var idx = msbp.Color_GetIndex(name);
		if (idx == -1)
			throw new Exception("Cannot set color of " + ColorPickerTarget);

		msbp.Color_Remove(name);
		msbp.Color_AddNew(name, c.ToMsbpColor());
		msbp.Color_MoveIndex(name, idx);

		ColorPickerTarget.Call("set_color", c);

		IsModified = true;
	}

	private void OnColorNameChanged(string oldName, string newName)
	{
		var msbp = ProjectManager.GetMSBP();

		// Get color instance
		var colorIdx = msbp.Color_GetIndex(oldName);
		if (colorIdx == -1)
			throw new NullReferenceException("Could not find color with name " + oldName);

		var color = msbp.Color_Get(colorIdx);

		// Ensure new name isn't already used
		if (msbp.Color_GetIndex(newName) != -1)
			throw new Exception("Requested name \"" + newName + "\" is already used!");

		msbp.Color_Remove(oldName);
		msbp.Color_AddNew(newName, color);
		msbp.Color_MoveIndex(newName, colorIdx);

		IsModified = true;
	}

	#endregion
}
