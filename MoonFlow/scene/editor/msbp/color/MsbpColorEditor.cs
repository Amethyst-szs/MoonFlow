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
	[Export]
	private Container NoExtensionWarning;

	private Node ColorPickerTarget = null;
	private bool IsEditFocusBottom = false;

	protected override void AppInit()
	{
		// Ensure extension is enabled
		ProjectConfig config = ProjectManager.GetConfig();
		if (config == null || !config.IsUseProjectExtensionColorPaletteEditor())
		{
			NoExtensionWarning.Show();
			return;
		}

		// Create color table
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		foreach (var color in colorResolver.ColorGradiationList)
			CreateAndAddColorEditor(color);

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
		int targetIdx = source.GetIndex();

		if (!colorResolver.IsColorNameUnique(newName))
			newName = CreateUniqueColorName(colorResolver);

		var gradiation = colorResolver.ColorGradiationList[targetIdx];
		gradiation.Name = newName;
		colorResolver.ColorGradiationList[targetIdx] = gradiation;

		IsModified = true;
	}

	private void OnAddNewColorPressed()
	{
		var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
		string newItemName = CreateUniqueColorName(colorResolver);

		ProjectColorResolver.ColorGradiation color = new (newItemName, Colors.White);
		colorResolver.ColorGradiationList.Add(color);

		CreateAndAddColorEditor(color);

		IsModified = true;
	}

	#endregion

	#region Utility

	private void CreateAndAddColorEditor(ProjectColorResolver.ColorGradiation info)
	{
		var element = ElementScene.Instantiate<HBoxContainer>();
		ElementHolder.AddChild(element);

		element.Call("setup", info.Name, info.Top, info.Bottom, info.IsGradient);

		element.Connect("name_modified", Callable.From(new Action<Node, string>(OnColorNameChanged)));
		element.Connect("color_top_picker_request", Callable.From(new Action<Node>(OnColorTopPickerRequest)));
		element.Connect("color_bottom_picker_request", Callable.From(new Action<Node>(OnColorBottomPickerRequest)));
	}

	private string CreateUniqueColorName(ProjectColorResolver resolver)
	{
		int newItemIdx = ElementHolder.GetChildCount();
		string newItemName = null;
		while (newItemName == null)
		{
			newItemName = "Color_" + newItemIdx.ToString();

			if (!resolver.IsColorNameUnique(newItemName))
			{
				newItemName = null;
				newItemIdx += 1;
			}
		}

		return newItemName;
	}

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
