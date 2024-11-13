using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nindot.LMS.Msbp;

public partial class MsbpFile : FileBase
{
    public bool Project_IsFileContainData() { return Project.IsValid(); }
    public int Project_GetSize() { return Project.GetSize(); }
    public ReadOnlyCollection<string> Project_GetContent() { return Project.GetContent(); }
    public string Project_GetElement(int idx) { return Project.GetElement(idx); }

    public void Project_AddElement(string value) { Project.AddElement(value); }
    public void Project_RemoveElement(string value) { Project.RemoveElement(value); }
}