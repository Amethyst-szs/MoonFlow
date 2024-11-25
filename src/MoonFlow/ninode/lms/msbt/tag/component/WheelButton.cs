using Godot;
using Nindot.LMS.Msbt.TagLib;
using System;

namespace MoonFlow.LMS.Msbt;

public partial class WheelButton : Button
{
	public MsbtTagElement ClickResult = null;
	public Texture2D Texture = null;

	public override void _Ready()
	{
		Pressed += OnPressed;
		MouseEntered += OnMouseEnter;
		MouseExited += OnMouseExit;
	}

	public static WheelButton Create(MsbtPageEditor editor, MsbtTagElement tag, string tooltip)
	{
		var b = new WheelButton
		{
			Texture = editor.GetTagTexture(tag),
			Name = tag.GetTagNameStr(),
			ClickResult = tag,
			TooltipText = tooltip,

			Flat = true,
			MouseDefaultCursorShape = CursorShape.PointingHand
		};

		b.Size = b.Texture.GetSize();
		b.CustomMinimumSize = b.Size;

		return b;
	}

	private void OnPressed()
	{

	}
	private void OnMouseEnter()
	{

	}
	private void OnMouseExit()
	{

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
		DrawTextureRect(Texture, new Rect2(-Size / (3 + (isHover ? 0 : 0.6F)), Size / (1.5F + (isHover ? 0 : 0.3F))), false);
    }
}
