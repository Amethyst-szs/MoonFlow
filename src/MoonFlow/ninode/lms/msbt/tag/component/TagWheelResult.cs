using Godot;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.LMS.Msbt;

public partial class TagWheelTagResult(MsbtTagElement tag) : RefCounted
{
    public readonly MsbtTagElement Tag = tag;
}