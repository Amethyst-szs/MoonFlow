using Godot;
using System;

using Nindot;

using MoonFlow.Project.Database;
using MoonFlow.Project;

namespace MoonFlow.Scene.Home;

public partial class MsbtFileListButton : DoubleClickButton
{
    public SarcFile FileArchive { get; private set; } = null;
    public string FileKey { get; private set; } = null;

    public void InitButton(SarcFile file, string key, Container box, WorldInfo world, string activeLanguage)
    {
        FileArchive = file;
        FileKey = key;

        ToggleMode = true;
		Name = key;
		Text = key;
		Alignment = HorizontalAlignment.Left;
        UpdateFileButtonModulation(activeLanguage);

        TooltipText = file.Name;
		if (world != null)
			TooltipText += '\n' + world.Display;

		if (box.IsInsideTree())
			FocusNeighborLeft = box.GetPath();
    }

    public void UpdateFileButtonModulation(string activeLanguage)
	{
		var meta = ProjectManager.GetMSBTMetaHolder(activeLanguage);
		if (meta == null)
			return;

		bool isEpoch = meta.IsLastModifiedTimeAtEpoch(FileArchive, FileKey);

		if (isEpoch) SelfModulate = Colors.Gray;
		else SelfModulate = Colors.White;
	}

    public bool IsDateAtUnixEpoch(string activeLanguage)
    {
        var meta = ProjectManager.GetMSBTMetaHolder(activeLanguage) ?? throw new NullReferenceException();
        var t = meta.GetLastModifiedTime(FileArchive, FileKey);
		return t.ToFileTimeUtc() == DateTime.UnixEpoch.ToFileTimeUtc();
    }
}
