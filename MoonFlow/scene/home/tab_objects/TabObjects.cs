using Godot;
using MoonFlow.Project;

using NaturalSort.Extension;

using System;
using System.ArrayExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoonFlow.Scene.Home;

[SceneUid("uid://cvdgsq1ltxug2")]
public partial class TabObjects : Control
{
    public List<ArchivePanel> SelectedList { get; private set; } = [];
    public ArchivePanel SelectedLast { get; private set; } = null;

    [Export, ExportGroup("Internal References")]
    private HFlowContainer FlowContent = null;

    #region Initilization

    public override void _Ready()
    {
        InitArchiveList();
    }

    private void InitArchiveList()
    {
        SelectedList.Clear();
        FlowContent.QueueFreeAllChildren();

        var list = GetArchiveNameList();
        foreach (var item in list)
            InitArchivePanel(item);

        SortArchivePanelList();
    }

    private void InitArchivePanel(string arc)
    {
        var panel = SceneCreator<ArchivePanel>.Create();
        panel.InitPanel(arc, GetArchiveDirectory());

        panel.Connect(ArchivePanel.SignalName.ArchiveClicked, Callable.From(
            new Action<ArchivePanel, bool, bool>(OnArchiveClicked)
        ));

        FlowContent.AddChild(panel);
    }

    #endregion

    #region Signals

    private void OnArchiveClicked(ArchivePanel panel, bool isCtrl, bool isShift)
    {
        if (!isCtrl && !isShift)
        {
            ClearSelectedArchives();
            SelectArchive(panel);
            return;
        }

        if (isShift)
        {
            if (SelectedLast == null)
                SelectArchive(panel);
            else
                RangeSelectArchives(panel, isCtrl);

            return;
        }

        if (isCtrl)
        {
            ToggleSelectArchive(panel);
            return;
        }
    }

    #endregion

    #region Utilities

    private static string GetArchiveDirectory() => ProjectManager.GetPath() + "ObjectData/";
    private static string[] GetArchiveNameList()
    {
        string path = GetArchiveDirectory();
        string[] list = [.. Directory.GetFiles(path).Select(s => s.Split(['\\', '/']).Last())];
        return list;
    }

    private void SelectArchive(ArchivePanel panel)
    {
        SelectedLast = panel;
        SelectedList.Add(panel);
        panel.Select();
    }
    private void DeselectArchive(ArchivePanel panel)
    {
        SelectedList.Remove(panel);
        panel.Deselect();
    }
    private void ToggleSelectArchive(ArchivePanel panel)
    {
        SelectedLast = panel;

        if (SelectedList.Remove(panel))
        {
            panel.Deselect();
        }
        else
        {
            SelectedList.Add(panel);
            panel.Select();
        }
    }
    private void RangeSelectArchives(ArchivePanel panel, bool isDeselect)
    {
        if (SelectedLast == null)
            return;

        // If range selecting the same panel just do a standard selection
        if (SelectedLast == panel)
        {
            SelectArchive(panel);
            return;
        }

        int rangeS = SelectedLast.GetIndex();
        int rangeE = panel.GetIndex();

        if (rangeE < rangeS)
            (rangeE, rangeS) = (rangeS, rangeE);

        // Select all panels in range
        for (int i = rangeS; i <= rangeE; i++)
        {
            if (isDeselect)
                DeselectArchive(FlowContent.GetChild<ArchivePanel>(i));
            else
                SelectArchive(FlowContent.GetChild<ArchivePanel>(i));
        }
    }
    private void ClearSelectedArchives()
    {
        SelectedLast = null;

        foreach (var item in SelectedList)
            item.Deselect();

        SelectedList.Clear();
    }

    private void SortArchivePanelList()
    {
        Node[] sorted = [.. FlowContent.GetChildren().OrderBy<Node, string>(
            s => s.Name, StringComparison.OrdinalIgnoreCase.WithNaturalSort()
        )];

        for (int i = 0; i < sorted.Length; i++)
            FlowContent.MoveChild(sorted[i], i);
    }

    #endregion
}
