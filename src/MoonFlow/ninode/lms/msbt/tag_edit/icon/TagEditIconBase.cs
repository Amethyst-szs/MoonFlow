using System;
using System.Numerics;
using Godot;
using Godot.Collections;
using Nindot.LMS.Msbt;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

public partial class TagEditIconBase : TagEditScene
{
	protected MsbtTagElement Tag = null;
	private MsbtPage Page = null;

	public override void SetupScene(MsbtTagElement tag, MsbtPage page)
	{
		Tag = tag;
		Page = page;
	}

	protected void SetupSubmenu(TagSubmenuBase menu)
	{
		menu.SetupPosition(Godot.Vector2.Zero);

		menu.Connect(TagSubmenuBase.SignalName.AddTag, Callable.From(new Action<Array<TagWheelTagResult>>(OnTagSelected)));
		menu.Connect(TagSubmenuBase.SignalName.TreeExited, Callable.From(OnMenuCancel));
	}

    public override bool IsRequirePageReference() { return true; }

    private void OnTagSelected(Array<TagWheelTagResult> tag)
	{
		if (tag.Count == 0)
		{
			QueueFree();
			return;
		}

		var result = tag[0].Tag;
		var replaceIndex = Page.IndexOf(Tag);

		if (replaceIndex == -1)
		{
			QueueFree();
			return;
		}
		
		Page[replaceIndex] = result;
		QueueFree();
	}

	private void OnMenuCancel()
	{
		QueueFree();
	}

	// Overrides default TagEditScene behavior
	public override void _Ready() { }
	public override void _UnhandledKeyInput(InputEvent @event) { }
}
