using System.Collections.Generic;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Project;

public class ProjectIconResolver(MsbtElementFactory factory, byte[] data, string name) : MsbtFile(factory, data, name)
{
    public static ProjectIconResolver FromPadStyleAndPadPair(byte[] padStyle, byte[] padPair)
    {
        // Create a main file out of PadStyle.msbt and a merge file out of PadPair.msbt
        var factory = new MsbtElementFactoryProjectSmo();
        var mainFile = new ProjectIconResolver(factory, padStyle, "ProjectIconResolver");
        var mergeFile = new MsbtFile(factory, padPair, "PairMerge");

        // Merge all entries from PadPair.msbt into PadStyle.msbt
        foreach (var entryL in mergeFile.GetEntryLabels())
        {
            var entry = mergeFile.GetEntry(entryL);
            mainFile.AddEntry(entryL, entry.CloneDeep());
        }

        // Return the main file as the ProjectIconResolver
        return mainFile;
    }

    public List<string> ResolveTextureNames(string key)
    {
        if (IsEdgeCaseLookupKey(key, out string edgeCase))
            return [edgeCase];
        
        var page = GetPage(key);
        if (page.Count == 0)
            return [];
        
        var result = new List<string>();

        foreach (var element in page)
        {
            if (element.GetType() != typeof(MsbtTagElementDeviceFont))
                continue;
            
            result.Add(((MsbtTagElementDeviceFont)element).GetTextureName(0));
        }

        return result;
    }
    
    public string ResolveText(string key)
    {
        if (IsEdgeCaseLookupKey(key, out string edgeCase))
            return null;
        
        var page = GetPage(key);
        if (page.Count == 0)
            return null;
        
        foreach (var element in page)
        {
            if (!element.IsText())
                continue;
            
            return element.GetText();
        }

        return null;
    }

    private MsbtPage GetPage(string key)
    {
        if (!Content.TryGetValue(key, out MsbtEntry entry))
            return [];
        
        if (entry.Pages.Count < 1)
            return [];
        
        return entry.Pages[0];
    }

    private bool IsEdgeCaseLookupKey(string key, out string edgeCase)
    {
        edgeCase = key;

        if (key == "ProjectTag" || key == "ProjectTag_CoinCollect")
            return true;

        return false;
    }
}