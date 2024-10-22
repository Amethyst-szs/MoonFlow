using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using MessageStudio.Formats.BinaryText;
using SarcLibrary;

namespace Nindot
{
    [GlobalClass]
    public partial class MsbtResource : Resource
    {
        public Msbt MsbtDict;

        // Constructor and Initilzation Functions

        public MsbtResource(Msbt file)
        {
            MsbtDict = file;
        }

        static public MsbtResource FromFilePath(string path)
        {
            Msbt msbt = MsbtFileAccess.ParseFile(path);
            return new MsbtResource(msbt);
        }

        static public MsbtResource FromSarc(SarcResource sarc, string file)
        {
            Sarc archive = sarc.SarcDict;
            if (!archive.ContainsKey(file))
                return null;

            byte[] data = archive[file].ToArray();

            Msbt msbt = MsbtFileAccess.ParseBytes(data);
            return new MsbtResource(msbt);
        }

        // GDScript Interfaces

        public int GetKeyCount()
        {
            return MsbtDict.Count;
        }

        public string[] GetKeys()
        {
            string[] keys = new string[MsbtDict.Count];
            MsbtDict.Keys.CopyTo(keys, 0);

            return keys;
        }

        public string[] GetTextValues()
        {
            MsbtEntry[] valueEntries = new MsbtEntry[MsbtDict.Count];
            MsbtDict.Values.CopyTo(valueEntries, 0);

            string[] values = [];
            foreach (MsbtEntry entry in valueEntries)
            {
                values.Append(entry.Text);
            }

            return values;
        }

        public string[] GetAttributeValues()
        {
            MsbtEntry[] valueEntries = new MsbtEntry[MsbtDict.Count];
            MsbtDict.Values.CopyTo(valueEntries, 0);

            string[] values = [];
            foreach (MsbtEntry entry in valueEntries)
            {
                values.Append(entry.Attribute);
            }

            return values;
        }

        public Godot.Collections.Dictionary GetDictionaryGDScript()
        {
            string[] keys = GetKeys();
            string[] values = GetTextValues();

            Godot.Collections.Dictionary dict = [];

            for (int i = 0; i < keys.Count(); i++)
            {
                dict.Add(keys[i], values[i]);
            }

            return dict;
        }

        public void SetValue(string key, string value)
        {
            MsbtDict[key].Text = value;
        }
    }
}