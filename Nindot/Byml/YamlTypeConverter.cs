using System;
using System.Collections.Generic;
using System.Numerics;

using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Nindot.Byml;

public class NindotCoreYamlTypeConverterBase
{
    protected static readonly Dictionary<Type, string> Table = new(){
        { typeof(bool), "!b" },
        { typeof(string), "!s" },
        { typeof(int), "!l" },
        { typeof(long), "!ll" },
        { typeof(uint), "!u" },
        { typeof(ulong), "!ul" },
        { typeof(float), "!f" },
        { typeof(double), "!d" },

        { typeof(Vector3), "" },
    };

    protected static void WriteYamlCoreTypes(IEmitter emitter, object value, Type type)
    {
        if (type == typeof(float)) // Fixes bug with locales that use commas as a decimal place separator
            emitter.Emit(new Scalar("⌂♯" + Table[type], null, ToStringNoLocale((float)value), ScalarStyle.Any, true, false));
        else if (type == typeof(double)) // Fixes bug with locales that use commas as a decimal place separator
            emitter.Emit(new Scalar("⌂♯" + Table[type], null, ToStringNoLocale((double)value), ScalarStyle.Any, true, false));
        else if (type == typeof(string) && value == null)
            emitter.Emit(new Scalar("⌂♯" + Table[type], null, "null", ScalarStyle.Any, true, false));
        else
            emitter.Emit(new Scalar("⌂♯" + Table[type], null, value.ToString(), ScalarStyle.Any, true, false));
    }
    protected static void WriteYamlVec3(IEmitter emitter, Vector3 value)
    {
        emitter.Emit(new MappingStart());

        emitter.Emit(new Scalar("X"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.X), ScalarStyle.Any, true, false));
        emitter.Emit(new Scalar("Y"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.Y), ScalarStyle.Any, true, false));
        emitter.Emit(new Scalar("Z"));
        emitter.Emit(new Scalar("⌂♯" + "!f", null, ToStringNoLocale(value.Z), ScalarStyle.Any, true, false));

        emitter.Emit(new MappingEnd());
    }

    protected static string ToStringNoLocale(float value) => value.ToString().Replace(',', '.');
    protected static string ToStringNoLocale(double value) => value.ToString().Replace(',', '.');
}
public class NindotCoreYamlTypeConverter : NindotCoreYamlTypeConverterBase, IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
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
        else
            WriteYamlCoreTypes(emitter, value, type);
    }
}