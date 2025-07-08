using System;
using System.Collections.Generic;
using System.Numerics;

namespace Nindot.Al.StageData;

public class StageObject : Dictionary<string, object>
{
    #region Property Readers

    public string GetId() => ReadString("Id");
    public string GetUnitConfigName() => ReadString("UnitConfigName");
    public string GetSuffix() => ReadString("Suffix");
    public string GetModelName() => ReadString("ModelName");
    public string GetLayerConfigName() => ReadString("LayerConfigName");

    public Dictionary<object, object> GetUnitConfig()
    {
        if (TryGetValue("UnitConfig", out object value) && value is Dictionary<object, object> dict)
            return dict;

        return null;
    }
    public string GetParameterConfigName() => ReadStringFromUnitConfig("ParameterConfigName");

    public Vector3 GetPosition() => ReadVector("Translate");
    public Vector3 GetRotate() => ReadVector("Rotate");
    public Vector3 GetScale() => ReadVector("Scale");

    #endregion

    #region Backend Util

    private string ReadString(string key)
    {
        if (TryGetValue(key, out object value) && value is string str)
            return str;

        return null;
    }
    private string ReadStringFromUnitConfig(string key)
    {
        Dictionary<object, object> unit = GetUnitConfig();
        if (unit == null)
            return null;

        if (unit.TryGetValue(key, out object value) && value is string str)
            return str;

        return null;
    }
    private Vector3 ReadVector(string key)
    {
        if (TryGetValue(key, out object value) && value is Dictionary<object, object> vecDict)
        {
            Vector3 vec = Vector3.Zero;

            if (vecDict.TryGetValue("X", out object x) && x is float xF)
                vec.X = xF;
            if (vecDict.TryGetValue("Y", out object y) && y is float yF)
                vec.Y = yF;
            if (vecDict.TryGetValue("Z", out object z) && z is float zF)
                vec.Z = zF;
            
            return vec;
        }

        return Vector3.Zero;
    }

    #endregion
}