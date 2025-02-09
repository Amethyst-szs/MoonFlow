using Godot;
using MoonFlow.Project;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass]
public partial class TagWheelButton : Button
{
	public enum ButtonTypes
	{
		None,

		Tag_SystemColor,
		Tag_SystemFont,
		Tag_SystemFontSize,
		Tag_EuiSpeed,
		Tag_EuiWait,
		Tag_Number,
		Tag_TextAnim,
		Tag_Voice,
		Tag_String,
		Tag_ProjectTag,
		Tag_TimeComponent,
		Tag_PictureFont,
		Tag_DeviceFont,
		Tag_TextAlign,
		Tag_Grammar,
		Tag_Ruby,
		Tag_Number_Time,
	};

	[Export]
	public ButtonTypes Type = ButtonTypes.None;
	[Export]
	public Texture2D Texture = null;

	[Signal]
	public delegate void AddTagEventHandler(TagWheelTagResult tag);
	[Signal]
	public delegate void AddSubmenuEventHandler(TagSubmenuBase menu);

	private static readonly Color ColorDefault = Color.FromHtml("#1a1a1a");
	private static readonly Color ColorFocus = Color.FromHtml("#7a3101");

	public override void _Ready()
	{
		if (Texture == null) return;

		Size = Texture.GetSize();
		Flat = true;
		MouseDefaultCursorShape = CursorShape.PointingHand;

		Pressed += OnPressed;
		MouseEntered += GrabFocus;
	}

	private void OnPressed()
	{
		switch (Type)
		{
			case ButtonTypes.Tag_SystemColor:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementSystemColor())]);
				return;
			case ButtonTypes.Tag_SystemFont:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementSystemFont())]);
				return;
			case ButtonTypes.Tag_SystemFontSize:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementSystemFontSize())]);
				return;
			case ButtonTypes.Tag_EuiSpeed:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementEuiSpeed())]);
				return;
			case ButtonTypes.Tag_EuiWait:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementEuiWait(20))]);
				return;
			case ButtonTypes.Tag_Number:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementNumberWithFigure(TagNameNumber.Score, "Score"))]);
				return;
			case ButtonTypes.Tag_TextAnim:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementTextAnim(TagNameTextAnim.Wave))]);
				return;
			case ButtonTypes.Tag_Voice:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementVoice())]);
				return;
			case ButtonTypes.Tag_String:
				var msbp = ProjectManager.GetMSBP();
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementString(msbp, "ReplaceString"))]);
				return;
			case ButtonTypes.Tag_ProjectTag:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementProjectTag(TagNameProjectIcon.ShineIconCurrentWorld))]);
				return;
			case ButtonTypes.Tag_TimeComponent:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementTimeComponent(TagNameTime.Second))]);
				return;
			case ButtonTypes.Tag_PictureFont:
				OpenSubmenu<TagSubmenuPictureFont>();
				return;
			case ButtonTypes.Tag_DeviceFont:
				OpenSubmenu<TagSubmenuDeviceFont>();
				return;
			case ButtonTypes.Tag_TextAlign:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementTextAlign())]);
				return;
			case ButtonTypes.Tag_Grammar:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementGrammar(TagNameGrammar.Cap))]);
				return;
			case ButtonTypes.Tag_Ruby:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementSystemRuby())]);
				return;
			case ButtonTypes.Tag_Number_Time:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementNumberTime(TagNameNumber.Date, "Date"))]);
				return;
		}
	}

	// ====================================================== //
	// ================ Sub Menu Instantiation ============== //
	// ====================================================== //

	private void OpenSubmenu<T>()
	{
		if (!typeof(T).IsSubclassOf(typeof(TagSubmenuBase)))
			throw new Exception(typeof(T).Name + " is invalid type!");

		var menu = SceneCreator<T>.Create();
		var scene = GetTree().CurrentScene;

		var menuBase = menu as TagSubmenuBase;
		scene.AddChild(menuBase);

		menuBase.InitSubmenu();

		EmitSignal(SignalName.AddSubmenu, menuBase);
	}

	// ====================================================== //
	// ====================== Rendering ===================== //
	// ====================================================== //

	public override void _Draw()
	{
		bool isActive = HasFocus();
		var buttonColor = isActive ? ColorFocus : ColorDefault;

		DrawSetTransform(Size / 2, 0, Vector2.One * (1.0F - (isActive ? 0.1F : 0.25F)));
		DrawCircle(Vector2.Zero, Size.X / 2F, buttonColor);

		if (IsInstanceValid(Texture))
			DrawTextureRect(Texture, new Rect2(-Size / 3F, Size / 1.5F), false);

		base._Draw();
	}
}
