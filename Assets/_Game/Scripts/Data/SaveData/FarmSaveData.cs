using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlacedItemSaveData
{
    public string itemId;
    public int originX;
    public int originY;
    public int originZ;
}

[Serializable]
public class FarmSaveData
{
    public List<PlacedItemSaveData> placedItems = new();
}