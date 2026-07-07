using System;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Project;
using MoonFlow.Async;
using MoonFlow.Scene.Main;
using MoonFlow.Addons;

namespace MoonFlow.Scene.EditorMsbt;

[SceneUid("uid://041p6wwk4vvx"), Icon("res://asset/nindot/lms/icon/System_Color_ForWheel.png")]
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
	private bool IsEditFocusBottom = false;

	protected override void AppInit()
	{
		// Create color table
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		foreach (var color in colorResolver.ColorGradiationList)
		{
			var element = ElementScene.Instantiate<HBoxContainer>();
			ElementHolder.AddChild(element);

			element.Call("setup", color.Name, color.Top, color.Bottom, color.IsGradient);

			element.Connect("name_modified", Callable.From(new Action<Node, string>(OnColorNameChanged)));
			element.Connect("color_top_picker_request", Callable.From(new Action<Node>(OnColorTopPickerRequest)));
			element.Connect("color_bottom_picker_request", Callable.From(new Action<Node>(OnColorBottomPickerRequest)));
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
		ProjectManager.GetMSBPHolder().WriteProjectDataArchive();

		// Reset flags
		display.UpdateProgress(1, 1);
		IsModified = false;
	}

	#endregion

	#region Signals

	private void OnColorTopPickerRequest(Node source)
	{
		// Assign picker target
		ColorPickerTarget = source;
		IsEditFocusBottom = false;

		// Set picker's current color
		ColorPicker.Color = GetTopColor(source);
		ColorPickerHolder.Position = (Vector2I)GetGlobalMousePosition();
		ColorPickerHolder.Popup();
	}
	private void OnColorBottomPickerRequest(Node source)
	{
		// Assign picker target
		ColorPickerTarget = source;
		IsEditFocusBottom = true;

		// Set picker's current color
		ColorPicker.Color = GetBottomColor(source);
		ColorPickerHolder.Position = (Vector2I)GetGlobalMousePosition();
		ColorPickerHolder.Popup();
	}

	private void OnColorPickerColorChanged(Color c)
	{
		if (ColorPickerTarget == null)
			return;
		
		bool isGradientEditMode = ColorPickerTarget.Call("is_gradient_mode").AsBool();
		int targetIdx = ColorPickerTarget.GetIndex();
	
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		var gradiation = colorResolver.ColorGradiationList[targetIdx];

		if (!isGradientEditMode)
		{
			gradiation.Top = c;
			gradiation.Bottom = c;
		}
		else
		{
			if (IsEditFocusBottom)
				gradiation.Bottom = c;
			else
				gradiation.Top = c;
		}

		ColorPickerTarget.Call("set_colors", gradiation.Top, gradiation.Bottom);

		colorResolver.ColorGradiationList[targetIdx] = gradiation;
		IsModified = true;
	}

	private void OnColorNameChanged(Node source, string newName)
	{
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		int targetIdx = ColorPickerTarget.GetIndex();

		var gradiation = colorResolver.ColorGradiationList[targetIdx];
		gradiation.Name = newName;
		colorResolver.ColorGradiationList[targetIdx] = gradiation;

		IsModified = true;
	}

	#endregion

	#region Utility

	private static Color GetTopColor(Node source)
	{
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		return colorResolver.GetTopColor(source.GetIndex());
	}
	private static Color GetBottomColor(Node source)
	{
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		return colorResolver.GetBottomColor(source.GetIndex());
	}

	#endregion
}
