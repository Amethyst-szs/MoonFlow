using Godot;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Scene.EditorMsbt;

public partial class TagWheelTagResult(MsbtTagElement tag) : RefCounted
{
    public readonly MsbtTagElement Tag = tag;
}