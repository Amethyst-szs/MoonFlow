using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot
{
    public partial class BymlIter : Dictionary<string, object>
    {
        public BymlIter(Dictionary<string, object> dict) : base(dict)
        {
        }

        public Type GetType(string key)
        {
            return this[key].GetType();
        }

        public Type GetType(int index)
        {
            return Values.ElementAt(index).GetType();
        }

        public bool TryGetType(out Type type, string key)
        {
            type = null;
            if (ContainsKey(key))
            {
                type = GetType(key);
                return true;
            }

            return false;
        }

        public bool TryGetValue<T>(out T value, string key)
        {
            value = default;
            if (!ContainsKey(key))
                return false;

            object obj = this[key];
            if (!obj.GetType().Equals(value))
                return false;

            value = (T)obj;
            return true;
        }

        public bool TryGetValue<T>(out T value, int index)
        {
            value = default;

            object obj = Values.ElementAt(index);
            if (!obj.GetType().Equals(value))
                return false;

            value = (T)obj;
            return true;
        }

        public bool TryGetIter(out BymlIter value, string key)
        {
            value = default;
            if (!ContainsKey(key))
                return false;

            object obj = this[key];
            if (obj.GetType() != typeof(Dictionary<object, object>))
                return false;

            Dictionary<object, object> dict = (Dictionary<object, object>)obj;
            Dictionary<string, object> dictS = dict.ToDictionary(k => k.Key.ToString(), k => k.Value);
            value = new BymlIter(dictS);

            return true;
        }

        public bool TryGetIter(out BymlIter value, int index)
        {
            value = default;

            object obj = Values.ElementAt(index);
            if (obj.GetType() != typeof(Dictionary<object, object>))
                return false;

            Dictionary<object, object> dict = (Dictionary<object, object>)obj;
            Dictionary<string, object> dictS = dict.ToDictionary(k => k.Key.ToString(), k => k.Value);
            value = new BymlIter(dictS);

            return true;
        }
    }
}