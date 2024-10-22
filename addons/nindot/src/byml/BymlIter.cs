using Godot;
using System;
using System.Collections.Generic;

namespace Nindot {
    public partial class BymlIter : Dictionary<object, object> {
        public BymlIter(Dictionary<object, object> dict) : base(dict) {
        }
    }
}