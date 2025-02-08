using System;
using System.Threading.Tasks;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;

using Nindot.Al.EventFlow;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Scene;

public static partial class AppSceneServer
{
    public static async Task<MsbtAppHolder> CreateOrOpenMsbtLabel(SarcMsbtFile file, string label)
    {
        return await CreateOrOpenMsbtLabel(file.Sarc.Name, file.Name, label);
    }
    public static async Task<MsbtAppHolder> CreateOrOpenMsbtLabel(NodeMessageResolverData resolver)
    {
        return await CreateOrOpenMsbtLabel(resolver.MessageArchive + ".szs", resolver.MessageFile + ".msbt", resolver.LabelName);
    }
    public static async Task<MsbtAppHolder> CreateOrOpenShineNameMsbt(string file, string label)
    {
        return await CreateOrOpenMsbtLabel("StageMessage.szs", file, label);
    }

    public static async Task<MsbtAppHolder> CreateOrOpenMsbtLabel(string archive, string file, string label)
    {
		// Lookup msbt editor application
		var apps = GetApps<MsbtAppHolder>();

		MsbtAppHolder app = null;
		var targetIdentifier = MsbtAppHolder.GetUniqueIdentifier(archive, file);

		foreach (var candidate in apps)
		{
			if (candidate.AppUniqueIdentifier != targetIdentifier)
				continue;

			app = candidate;
			break;
		}

        // Get msbt file
        var sarc = ProjectManager.GetMSBTArchives().GetArchiveByFileName(archive, false);
        if (sarc == null || !sarc.Content.ContainsKey(file))
            return null;
        
        var msbt = sarc.GetFileMSBT(file, new MsbtElementFactory());

		// Check if the requested label exists
		if (msbt.IsContainKey(label))
		{
			// If the pre-existing msbt app lookup failed, create a new editor app
			if (app == null)
			{
				app = await MsbtAppHolder.OpenAppWithSearch(archive, file, label);
				app.Editor.SetSelection(label);
				return app;
			}

			app.Editor.UpdateEntrySearch(label);
			app.Editor.SetSelection(label);
			app.AppFocus();

			return app;
		}

		// If the label doesn't already exist, we'll need to create it
		if (app == null)
		{
			app = await MsbtAppHolder.OpenApp(archive, file);
			await app.ToSignal(app.GetTree().CreateTimer(0.2), Godot.Timer.SignalName.Timeout);

			app.Editor.OnAddEntryNameSubmitted(label);
			app.Editor.UpdateEntrySearch(label);
			app.AppFocus();

			return app;
		}
		
		app.Editor.OnAddEntryNameSubmitted(label);
		app.Editor.UpdateEntrySearch(label);
		app.AppFocus();

        return app;
    }
}