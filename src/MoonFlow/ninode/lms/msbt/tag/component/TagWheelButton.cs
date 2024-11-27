using Godot;
using Godot.Collections;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

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
		Tag_TextAnim,
		Tag_ProjectTag,
		Tag_PictureFont,
		Tag_DeviceFont,
		Tag_TextAlign,
		Tag_Voice,

		ShowMore,
	};

	[Export]
	public ButtonTypes Type = ButtonTypes.None;

	[Export]
	public Texture2D Texture = null;

	[Signal]
	public delegate void AddTagEventHandler(TagWheelTagResult tag);
	[Signal]
	public delegate void AddSubMenuEventHandler();

	public override void _Ready()
	{
		if (Texture == null) return;

		Size = Texture.GetSize();
		Flat = true;
		MouseDefaultCursorShape = CursorShape.PointingHand;

		Pressed += OnPressed;
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
			case ButtonTypes.Tag_TextAnim:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementTextAnim(TagNameTextAnim.Wave))]);
				return;
			case ButtonTypes.Tag_ProjectTag:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementProjectTag(TagNameProjectIcon.ShineIconCurrentWorld))]);
				return;
			case ButtonTypes.Tag_PictureFont:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementPictureFont(TagNamePictureFont.COMMON_MARIO))]);
				return;
			case ButtonTypes.Tag_DeviceFont:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementDeviceFont(TagNameDeviceFont.ButtonA))]);
				return;
			case ButtonTypes.Tag_TextAlign:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementTextAlign())]);
				return;
			case ButtonTypes.Tag_Voice:
				EmitSignal(SignalName.AddTag, [new TagWheelTagResult(new MsbtTagElementVoice())]);
				return;
			case ButtonTypes.ShowMore:
				EmitSignal(SignalName.AddSubMenu, []);
				return;
			default:
				return;
		}
	}

	// ====================================================== //
	// ====================== Rendering ===================== //
	// ====================================================== //

	public override void _Draw()
	{
		bool isHover = IsHovered();

		var buttonColor = new Color(0.1F, 0.1F, 0.1F, 1.0F);
		if (isHover) buttonColor *= 2;

		DrawSetTransform(Size / 2);
		DrawCircle(Vector2.Zero, Size.X / (2 + (isHover ? 0 : 0.3F)), buttonColor);

		if (IsInstanceValid(Texture))
			DrawTextureRect(Texture, new Rect2(-Size / (3 + (isHover ? 0 : 0.6F)), Size / (1.5F + (isHover ? 0 : 0.3F))), false);
	}
}
