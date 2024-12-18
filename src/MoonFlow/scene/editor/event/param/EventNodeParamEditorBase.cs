using System;
using Godot;
using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

public abstract partial class EventNodeParamEditorBase : HBoxContainer
{
    protected EventFlowNode Node = null;
    protected string Param = null;

    protected Button ButtonAddProperty { get; private set; }
    private Texture2D ButtonAddTexture = GD.Load<Texture2D>("res://asset/material/file/add.svg");

    public void InitEditor(EventFlowNode node, string param)
    {
        Node = node;
        Param = param;

        ButtonAddProperty = new Button()
        {
            Icon = ButtonAddTexture,
            MouseFilter = MouseFilterEnum.Pass,
        };

        ButtonAddProperty.AddThemeConstantOverride("icon_max_width", 16);
        ButtonAddProperty.Pressed += AddPropertyToNode;

        AddChild(ButtonAddProperty);

        Init();
    }

    public abstract void Init();
    public virtual void AddPropertyToNode()
    {
        ButtonAddProperty.QueueFree();
        Node.ParamAddDropdownHolder.RemoveChild(this);
        Node.ParamHolder.AddChild(this);
    }
}