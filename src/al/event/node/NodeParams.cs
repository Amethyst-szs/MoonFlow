using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow;

public class NodeParams : Dictionary<object, object>
{
    public NodeParams() { }
    public NodeParams(Dictionary<object, object> iter) : base(iter) { }
}