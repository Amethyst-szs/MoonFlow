using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

using MoonFlow.Async;

namespace MoonFlow.Project.Cache;

public class ProjectLabelCache()
{
    private readonly Dictionary<ArchiveType, Dictionary<string, ReadOnlyCollection<string>>> LabelList = [];
    private int TaskProgress = 0;
    private int TaskCount = 0;

    public enum ArchiveType
    {
        SYSTEM,
        STAGE,
        LAYOUT,
    }

    #region Cache Updater

    public async void UpdateCache()
    {
        GD.Print("Updating ProjectLabelCache...");
        var run = AsyncRunner.Run(TaskRunUpdateCache, AsyncDisplay.Type.UpdateProjectLabelCache);

        await run.Task;

        GD.Print("Completed ProjectLabelCache update");
    }

    public void UpdateCacheSynchronous()
    {
        GD.Print("Updating ProjectLabelCache... (Synchronous)");
        TaskRunUpdateCache(null);
        GD.Print("Completed ProjectLabelCache update");
    }

    private void TaskRunUpdateCache(AsyncDisplay display)
    {
        var holder = ProjectManager.GetMSBTArchives();

        // Count total tasks
        TaskProgress = 0;
        TaskCount = 0;
        TaskCount += holder.SystemMessage.Content.Count;
        TaskCount += holder.StageMessage.Content.Count;
        TaskCount += holder.LayoutMessage.Content.Count;

        display?.UpdateProgress(TaskProgress, TaskCount);

        // Update each archive's cache
        LabelList[ArchiveType.SYSTEM] = UpdateArchiveCache(holder.SystemMessage, display);
        LabelList[ArchiveType.STAGE] = UpdateArchiveCache(holder.StageMessage, display);
        LabelList[ArchiveType.LAYOUT] = UpdateArchiveCache(holder.LayoutMessage, display);
    }

    private Dictionary<string, ReadOnlyCollection<string>> UpdateArchiveCache(SarcFile arc, AsyncDisplay display)
    {
        var dict = new Dictionary<string, ReadOnlyCollection<string>>();
        
        foreach (var data in arc.Content)
        {
            MsbtFile file;

            try
            {
                file = MsbtFile.FromBytes([.. data.Value], data.Key, new MsbtElementFactory());
            }
            catch
            {
                GD.PushWarning("Failed to read cache for ", data.Key);
                IncrementTaskProgress(display);
                continue;
            }

            var labels = file.GetEntryLabels();
            dict[data.Key] = labels;

            IncrementTaskProgress(display);
        }

        return dict;
    }

    private void IncrementTaskProgress(AsyncDisplay display)
    {
        TaskProgress += 1;
        display?.UpdateProgress(TaskProgress, TaskCount);
    }

    #endregion
}