using System.Linq;

namespace Nindot.Al.EventFlow;

public class ItemEntry
{
    private string Name;

    public ItemEntry(int databaseIndex)
    {
        SetName(databaseIndex);
    }
    public ItemEntry(string itemName)
    {
        SetName(itemName);
    }

    public string GetDisplayName()
    {
        ItemTranslationTable.Table.TryGetValue(Name, out string nEnglish);
        if (nEnglish == null || nEnglish.Length == 0)
            return Name;
        
        return nEnglish;
    }
    public string GetName()
    {
        return Name;
    }

    public void SetName(int databaseIndex)
    {
        if (databaseIndex >= ItemTranslationTable.Table.Count)
            return;
        
        Name = ItemTranslationTable.Table.Keys.ElementAt(databaseIndex);
    }
    public void SetName(string itemName)
    {
        Name = itemName;
    }
}