using Godot;
using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

public partial class OptionStageName : OptionButton
{
	private WorldShineEditor Parent = null;
	private string SelectionText = null;

	[Signal]
	public delegate void StageSelectedEventHandler(string name);

	public override void _Ready()
	{
		VisibilityChanged += SetupOptionList;
		ItemSelected += SetSelectionIdx;
	}

	public void SetSelection(WorldShineEditor parent, string stage)
	{
		// Copy reference to parent
		Parent = parent;
		SetupOptionList();

		SetSelection(stage);
	}

	private void SetupOptionList()
	{
		Clear();

		var cat = StageInfo.CatEnum.Unknown;

		foreach (var stage in Parent.World.StageList)
		{
			if (stage.CategoryType != cat)
			{
				cat = stage.CategoryType;
				AddSeparator(StageInfo.CategoryNames[(int)cat]);
			}

			AddItem(stage.name, Parent.World.StageList.IndexOf(stage));
		}

		SetSelection(SelectionText);
	}

	private void SetSelectionIdx(long idx)
	{
		SelectionText = GetItemText((int)idx);
		Selected = (int)idx;

		EmitSignal(SignalName.StageSelected, SelectionText);
	}

	private void SetSelection(string name, bool isEmit = false)
	{
		SelectionText = name;

		var id = Parent.World.StageList.FindIndex(s => s.name == name);
		if (id < 0)
			id = 0;

		Selected = GetItemIndex(id);

		if (isEmit)
			EmitSignal(SignalName.StageSelected, SelectionText);
	}
}
