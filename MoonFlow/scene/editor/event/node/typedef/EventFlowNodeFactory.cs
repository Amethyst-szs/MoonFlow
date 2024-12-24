using System;
using System.Collections.Generic;
using Godot;

using Nindot.Al.EventFlow.Smo;

namespace MoonFlow.Scene.EditorEvent;

public static class EventFlowNodeFactory
{
    private static readonly Dictionary<Type, string> FactoryEntries = new(){
        { typeof(NodeFork), "fork/fork.tscn" },
        { typeof(NodeJoin), "join/join.tscn" },
        { typeof(NodeJumpEntry), "jump/entry_point_jump.tscn" },

        { typeof(NodeMessageBalloon), "balloon/balloon.tscn" },
        { typeof(NodeMessageTalk), "talk/talk.tscn" },
        { typeof(NodeSelectChoice), "" },
        { typeof(NodeSelectYesNo), "" },
    };

    private const string PathBase = "res://scene/editor/event/node/typedef/";

    public static EventFlowNodeCommon Create<T>() where T : Nindot.Al.EventFlow.Node
    {
        // Ensure this string exists in the factory table
        if (!FactoryEntries.TryGetValue(typeof(T), out string path))
            return SceneCreator<EventFlowNodeCommon>.Create();
        
        if (path == string.Empty)
            return SceneCreator<EventFlowNodeCommon>.Create();

        var scene = GD.Load<PackedScene>(PathBase + path);
        var node = scene.Instantiate();
        return node as EventFlowNodeCommon;
    }
}