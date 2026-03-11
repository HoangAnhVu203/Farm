using System;
using System.Collections.Generic;

[Serializable]
public class SoilPlotSaveData
{
    public string plotKey;
    public bool isPlanted;
    public string seedId;
    public long plantedUnixTime;
}

[Serializable]
public class SoilSaveFile
{
    public List<SoilPlotSaveData> plots = new();
}