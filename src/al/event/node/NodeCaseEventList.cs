using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class NodeCaseEventList
{
    // ====================================================== //
    // ============ Initilization and Parameters ============ //
    // ====================================================== //

    private List<NodeCaseEvent> CaseList = [];

    public NodeCaseEventList(List<object> array)
    {
        // Initilize all items in array
        foreach (var obj in array)
        {
            if (obj.GetType() != typeof(Dictionary<object, object>))
                continue;
            
            var item = (Dictionary<object, object>)obj;
            CaseList.Add(new NodeCaseEvent(item));
        }
    }

    public int GetCaseCount() { return CaseList.Count; }
    public int GetCaseNextId(int index) { return CaseList[index].NextId; }
    public int[] GetCaseNextIdList() {
        List<int> ids = [];

        foreach (var c in CaseList)
        {
            if (c.NextId != int.MinValue)
                ids.Add(c.NextId);
        }

        return [.. ids];
    }

    public void SetNextNodeForCase(Node node, int caseIndex)
    {
        SetCaseListSize(caseIndex + 1);
        CaseList[caseIndex].NextId = node.GetId();
    }
    public void RemoveNextNodeForCase(int caseIndex)
    {
        if (caseIndex >= CaseList.Count)
            return;
        
        CaseList[caseIndex].NextId = int.MinValue;
    }

    private void SetCaseListSize(int size)
    {
        int idx = CaseList.Count - 1;
        while (CaseList.Count < size)
        {
            CaseList.Add(new NodeCaseEvent(idx));
            idx += 1;
        }
    }

    // ====================================================== //
    // =========== List item structure and content ========== //
    // ====================================================== //

    public class NodeCaseEvent
    {
        public int Index;
        public int NextId;
        public string Name;

        public MessageResolverData MessageData;

        public NodeCaseEvent(int index)
        {
            Index = index;
            NextId = int.MinValue;
        }
        public NodeCaseEvent(int index, int nextId)
        {
            Index = index;
            NextId = nextId;
        }
        public NodeCaseEvent(Dictionary<object, object> dict)
        {
            if (dict.ContainsKey("Index")) Index = (int)dict["Index"];
            if (dict.ContainsKey("NextId")) NextId = (int)dict["NextId"];
            if (dict.ContainsKey("Name")) Name = (string)dict["Name"];

            if (dict.ContainsKey("MessageData"))
                MessageData = new((Dictionary<object, object>)dict["MessageData"]);
        }

        public class MessageResolverData
        {
            public string MessageArchive;
            public string MessageFile;
            public string LabelName;

            public MessageResolverData(Dictionary<object, object> dict)
            {
                if (dict.ContainsKey("MessageType")) MessageArchive = (string)dict["MessageType"];
                if (dict.ContainsKey("MessageFileName")) MessageFile = (string)dict["MessageFileName"];
                if (dict.ContainsKey("LabelName")) LabelName = (string)dict["LabelName"];
            }
        }
    }
}