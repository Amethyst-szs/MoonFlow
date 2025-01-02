using System;
using Godot;

namespace MoonFlow.Scene;

public static class BitFlagButtonHolder
{
    public static int GetValue(VBoxContainer node) { return node.Get("value").As<int>(); }
    public static void SetValue(VBoxContainer node, int value) { node.Call("set_value", value); }
    public static int GetPrimaryBit(VBoxContainer node) { return node.Get("primary_bit").As<int>(); }
    public static void SetPrimaryBit(VBoxContainer node, int value) { node.Call("set_primary_bit", value); }
    public static void ConnectValueChanged(VBoxContainer node, Action<int> action)
    {
        var call = Callable.From(action);

        if (!node.IsConnected("value_changed", call))
            node.Connect("value_changed", call);
    }
    public static void ConnectPrimaryBitChanged(VBoxContainer node, Action<int> action)
    {
        var call = Callable.From(action);

        if (!node.IsConnected("primary_bit_changed", call))
            node.Connect("primary_bit_changed", call);
    }
}