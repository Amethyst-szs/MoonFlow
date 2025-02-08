using Godot;
using System;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEditorAltCode : PanelContainer
{
	[Export]
	private Label LabelPreview;
	private string Code = null;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Hide();

		LabelPreview.Text = string.Empty;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsEcho())
			return;

		if (@event is InputEventKey key)
			HandleAltInput(key);
	}

	private void HandleAltInput(InputEventKey key)
	{
		// If releasing the alt key, send the fake unicode input
		if (!key.IsPressed() && key.Keycode == Key.Alt)
		{
			SubmitCode();
			Hide();

			Code = null;
			LabelPreview.Text = string.Empty;
			return;
		}

		if (!key.IsPressed() || key.GetModifiersMask() != KeyModifierMask.MaskAlt)
			return;
		
		Code ??= "";

		// If keycode isn't 0-9 or A-F, check for backspace or show request. Otherwise return
		if (!IsKeycodeHexValue(key.Keycode))
		{
			if (key.Keycode == Key.Backspace)
				Code = Code[..^1];
			else if (key.Keycode == Key.U)
				Show();
			
			LabelPreview.Text = Code;
			return;
		}

		Code += key.AsTextKeyLabel().Replace("Alt+", "").ToUpper();

		Show();
		LabelPreview.Text = Code;
	}

	private void SubmitCode()
	{
		var focus = GetViewport().GuiGetFocusOwner();
		if (Code == null || focus == null || focus is not TextEdit textEdit)
			return;
		
		try
		{
			// Case string to code and ensure codepoint
			var code = Convert.ToInt32(Code, 16);
			if (!(code <= 0x10FFFF && (code < 0xD800 || code > 0xDFFF)))
				return;
			
			// Test to see if char conversion will be successful
			if (!char.IsSymbol(char.ConvertFromUtf32(code), 0))
				return;

			textEdit._HandleUnicodeInput(code, 0);
		}
		catch (Exception e)
		{
			GD.PushWarning("Invalid input for alt-code!\n", e.Message);
		}
	}

	private static bool IsKeycodeHexValue(Key k)
	{
		return (k >= Key.Key0 && k <= Key.Key9) || (k >= Key.A && k <= Key.F);
	}
}
