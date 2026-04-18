using System;
using System.Collections.Generic;

[Serializable]
public class PreparedServiceItemSelection
{
    public string requirementId;
    public string productUniqueId;
    public string productId;
}

[Serializable]
public class PreparedServiceLoadout
{
    public string serviceId;
    public List<PreparedServiceItemSelection> selections = new List<PreparedServiceItemSelection>();

    public PreparedServiceItemSelection GetSelectionByRequirement(string requirementId)
    {
        return selections.Find(x => x.requirementId == requirementId);
    }
}