using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class NodeCaseEventList
{
    // ====================================================== //
    // ============ Initilization and Parameters ============ //
    // ====================================================== //

    public List<NodeCaseEvent> CaseList { get; private set; } = [];

    public NodeCaseEventList() { }
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

    internal List<Dictionary<string, object>> WriteBuild()
    {
        var list = new List<Dictionary<string, object>>();

        foreach (var c in CaseList)
        {
            var dict = new Dictionary<string, object>
            {
                ["NextId"] = c.NextId
            };

            if (c.Index != int.MinValue)
                dict["Index"] = c.Index;

            if (c.Name != null)
                dict["Name"] = c.Name;

            if (c.MessageData != null)
                dict["MessageData"] = c.MessageData.WriteBuild();

            list.Add(dict);
        }

        return list;
    }

    public int GetCaseCount() { return CaseList.Count; }
    public int GetCaseNextId(int index) { return CaseList[index].NextId; }
    public int[] GetCaseNextIdList()
    {
        List<int> ids = [];

        foreach (var c in CaseList)
            ids.Add(c.NextId);

        return [.. ids];
    }

    public void SetNextNodeForCase(Node node, int caseIndex)
    {
        TryIncreaseCaseListSize(caseIndex + 1);
        CaseList[caseIndex].SetNextId(node);
    }
    public void RemoveNextNodeForCase(int caseIndex)
    {
        if (caseIndex >= CaseList.Count)
            return;

        CaseList[caseIndex].SetNextId(null);
    }

    public void TryIncreaseCaseListSize(int size)
    {
        int idx = CaseList.Count;
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
        public readonly int Index = int.MinValue;
        public int NextId { get; private set; }
        public string Name;

        public NodeMessageResolverData MessageData;

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

        public void SetNextId(Node node)
        {
            if (node == null)
            {
                NextId = int.MinValue;
                return;
            }

            NextId = node.GetId();
        }
    }
}