using System;
using Godot;

namespace MoonFlow.Scene.EditorEvent;

public static class EventNodeParamFactory
{
    private static EventNodeParamEditorBase Build(Type type)
    {
        return type.Name switch
        {
            "Boolean" => SceneCreator<ParamEditorBoolean>.Create(),
            "Int32" => SceneCreator<ParamEditorInt>.Create(),
            "Single" => SceneCreator<ParamEditorFloat>.Create(),
            "String" => SceneCreator<ParamEditorString>.Create(),
            _ => null,
        };
    }

    public static void CreateParamEditor(EventFlowNodeCommon parent, string param, Type paramType)
    {
        EventNodeParamEditorBase editor = Build(paramType);
        if (editor == null)
            return;
        
        editor.InitEditor(parent, param);

        if (parent.Content.IsParamDefined(param))
            parent.ParamHolder.AddChild(editor);
        else
            parent.ParamAddDropdownHolder.AddChild(editor);
    }
}