using System;
using System.Collections.Generic;
using System.Numerics;

using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

using Nindot.Byml;

namespace MoonFlow.Project.Byml;

public class YamlTypeConverterEx : NindotCoreYamlTypeConverterBase, IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        if (type == typeof(Godot.Color))
            return true;

        return Table.ContainsKey(type);
    }
    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        throw new NotImplementedException();
    }
    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        if (type == typeof(Vector3))
            WriteYamlVec3(emitter, (Vector3)value);
        else if (type == typeof(Godot.Color))
            WriteYamlGodotColor(emitter, (Godot.Color)value);
        else
            WriteYamlCoreTypes(emitter, value, type);
    }

    private static void WriteYamlGodotColor(IEmitter emitter, Godot.Color value)
    {
        emitter.Emit(new MappingStart());

        emitter.Emit(new Scalar("R"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.R), ScalarStyle.Any, true, false));
        emitter.Emit(new Scalar("G"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.G), ScalarStyle.Any, true, false));
        emitter.Emit(new Scalar("B"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.B), ScalarStyle.Any, true, false));
        emitter.Emit(new Scalar("A"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.A), ScalarStyle.Any, true, false));

        emitter.Emit(new MappingEnd());
    }
}