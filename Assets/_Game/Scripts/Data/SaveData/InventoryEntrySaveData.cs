using System;
using System.Collections.Generic;

[Serializable]
public class InventoryEntrySaveData
{
    public string itemId;
    public int amount;
}

[Serializable]
public class InventorySaveData
{
    public List<InventoryEntrySaveData> entries = new();
}