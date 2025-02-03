using System;
using System.Collections.Generic;
using Godot;

using Nindot.Al.EventFlow.Smo;

namespace MoonFlow.Scene.EditorEvent;

public static class EventFlowNodeFactory
{
    private static readonly Dictionary<Type, string> FactoryEntries = new(){
        { typeof(NodeFork), "flow/fork.tscn" },
        { typeof(NodeJoin), "flow/join.tscn" },
        { typeof(NodeJumpEntry), "flow/entry_point_jump.tscn" },

        { typeof(NodeCapMessage), "cap_message/cap_message.tscn" },
        { typeof(NodeMessageBalloon), "balloon/balloon.tscn" },
        { typeof(NodeMessageTalk), "talk/talk.tscn" },
        { typeof(NodeSelectChoice), "talk/choice.tscn" },
        { typeof(NodeSelectYesNo), "talk/choice_bool.tscn" },

        { typeof(NodeEventQuery), "event/event_query.tscn" },
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