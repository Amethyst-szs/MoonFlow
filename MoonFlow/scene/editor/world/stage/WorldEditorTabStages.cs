using Godot;
using System;

using MoonFlow.Project.Database;
using MoonFlow.Project;
using System.Linq;

namespace MoonFlow.Scene.EditorWorld;

public partial class WorldEditorTabStages : VBoxContainer
{
    private WorldEditorApp Parent = null;
    private WorldInfo World = null;

    private string NewStageName = "";
    private StageInfo.CatEnum NewStageCategory = StageInfo.CatEnum.ExStage;

    [Export, ExportGroup("Internal References")]
    private VBoxContainer StageList;
    [Export]
    private OptionStageType OptionStageType;
    [Export]
	private Label LabelNewStageError;

    public void Init(WorldInfo world)
    {
        Parent = this.FindParentByType<WorldEditorApp>();
        World = world;

        // Setup name input field
        OnNewStageNameChanged(NewStageName);

        // Set default value of stage type
        OptionStageType.Selected = (int)NewStageCategory;

        SetupStageList();
    }
    private void SetupStageList()
    {
        StageList.QueueFreeAllChildren();

        var prevCategory = StageInfo.CatEnum.Unknown;
        HFlowContainer currentFlowbox = null;

        foreach (var stage in World.StageList)
        {
            // If switched to a new category, write our flowbox, create a header, and make a new flowbox
            if (stage.CategoryType != prevCategory)
            {
                prevCategory = stage.CategoryType;

                var catName = StageInfo.CategoryNames[(int)stage.CategoryType];
                var separator = SceneCreator<StageTypeSeparator>.Create().Init(catName);
                StageList.AddChild(separator);

                currentFlowbox = new();
                StageList.AddChild(currentFlowbox);
            }

            var infoEdit = SceneCreator<EditStageInfo>.Create();

            infoEdit.Connect(EditStageInfo.SignalName.RefreshList, Callable.From(() =>
            {
                Parent.OnWorldInfoModify();
                SetupStageList();
            }));

            currentFlowbox.AddChild(infoEdit);
            infoEdit.Setup(World, stage);
        }
    }

    #region Signals

    private void OnNewStageNameChanged(string str)
    {
        NewStageName = str;

        bool isValid = IsNewStageNameValid(out string errorSource);
        LabelNewStageError.Visible = !isValid && errorSource != "empty";

        if (isValid || str == string.Empty)
            return;

        LabelNewStageError.Text = Tr("WORLD_EDITOR_INVALID_NEW_STAGE_NAME_ERROR") + " " + errorSource;
    }

    private void OnNewStageCategoryChanged(int id)
    {
        NewStageCategory = (StageInfo.CatEnum)id;
    }

    private void OnNewStageSubmitted()
    {
        if (!IsNewStageNameValid(out _))
            return;

        // Create new StageInfo
        var info = new StageInfo
        {
            name = NewStageName,
            CategoryType = NewStageCategory,
        };

        World.StageList.Add(info);
        ProjectDatabaseHolder.SortWorldStagesByType(World.StageList);

        // Reload list
        SetupStageList();

        Parent.OnWorldInfoModify();
    }

    #endregion
    
    #region Utilities

	private bool IsNewStageNameValid(out string errorSource)
	{
		if (NewStageName == string.Empty)
		{
			errorSource = "empty";
			return false;
		}

		// Check if this world already has this name
		if (World.StageList.Any((s) => s.name == NewStageName))
		{
			errorSource = World.Display;
			return false;
		}

		// Check if any world already has this stage name
		foreach (var world in ProjectManager.GetDB().WorldList)
		{
			if (world.StageList.Any((s) => s.name == NewStageName))
			{
				errorSource = world.Display;
				return false;
			}
		}

		errorSource = "";
		return true;
	}

	#endregion
}
