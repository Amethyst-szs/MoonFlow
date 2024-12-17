using System;
using System.IO;

using Nindot.Byml;

namespace Nindot.Al.EventFlow;

public class SarcEventFlowGraph(BymlFile byml, string name, EventFlowFactoryBase nodeFactory, SarcFile sarc)
    : Graph(byml, name, nodeFactory)
{
    public SarcFile Sarc { get; private set; } = sarc;

    public Exception WriteArchive()
    {
        if (!Sarc.Content.ContainsKey(Name))
            throw new SarcFileException("Missing MsbtFile key!");

        if (!WriteBytes(out byte[] data))
            throw new EventFlowException("Failed to write EventFlowGraph");

        Sarc.Content[Name] = data;
        return Sarc.WriteArchive();
    }
}