using Dalamud.Configuration;
using System;
using System.Collections.Generic;


namespace SRankAssistant;

internal sealed class Tracker : IPluginConfiguration
{
    public int Version { get; set; }
    public readonly Dictionary<uint, Dictionary<uint, Dictionary<uint, uint>>> InstancedKillGatherCounts = new();
    public readonly Dictionary<uint, Dictionary<uint, Dictionary<uint, DateTime>>> InstancedFateSuccessTimes = new();
    public uint DiscardCount;

    public static Tracker Load() => SERVICES.Interface.GetPluginConfig() as Tracker ?? new Tracker();
    public void Save() => SERVICES.Interface.SavePluginConfig(this);

    public uint GetCount(uint target) => InstancedKillGatherCounts.TryGetValue(SERVICES.ClientState.Instance, out Dictionary<uint, Dictionary<uint, uint>>? instanceDictI) && instanceDictI.TryGetValue(SERVICES.ClientState.TerritoryType, out Dictionary<uint, uint>? zoneDictI) && zoneDictI.TryGetValue(target, out uint countValueI) ? countValueI : 0;

    public void Increment(uint target)
    {
        if (!InstancedKillGatherCounts.ContainsKey(SERVICES.ClientState.Instance)) InstancedKillGatherCounts[SERVICES.ClientState.Instance] = new Dictionary<uint, Dictionary<uint, uint>>();
        Dictionary<uint, Dictionary<uint, uint>> instanceDict = InstancedKillGatherCounts[SERVICES.ClientState.Instance];
        if (!instanceDict.ContainsKey(SERVICES.ClientState.TerritoryType)) instanceDict[SERVICES.ClientState.TerritoryType] = new Dictionary<uint, uint>();
        Dictionary<uint, uint> zoneDict = instanceDict[SERVICES.ClientState.TerritoryType];
        if (!zoneDict.ContainsKey(target)) zoneDict[target] = 0;
        zoneDict[target]++;
        Save();
    }

    public DateTime? GetFateLastSuccess(uint fateId) => InstancedFateSuccessTimes.TryGetValue(SERVICES.ClientState.Instance, out Dictionary<uint, Dictionary<uint, DateTime>>? instanceFateDictI) && instanceFateDictI.TryGetValue(SERVICES.ClientState.TerritoryType, out Dictionary<uint, DateTime>? zoneFateDictI) && zoneFateDictI.TryGetValue(fateId, out DateTime timeValueI) ? timeValueI : null;

    public void RecordFateSuccess(uint fateId)
    {
        if (!InstancedFateSuccessTimes.ContainsKey(SERVICES.ClientState.Instance)) InstancedFateSuccessTimes[SERVICES.ClientState.Instance] = new Dictionary<uint, Dictionary<uint, DateTime>>();
        Dictionary<uint, Dictionary<uint, DateTime>> instanceDict = InstancedFateSuccessTimes[SERVICES.ClientState.Instance];
        if (!instanceDict.ContainsKey(SERVICES.ClientState.TerritoryType)) instanceDict[SERVICES.ClientState.TerritoryType] = new Dictionary<uint, DateTime>();
        instanceDict[SERVICES.ClientState.TerritoryType][fateId] = DateTime.UtcNow;
        Save();
    }

    public void ResetZone()
    {
        if (InstancedKillGatherCounts.TryGetValue(SERVICES.ClientState.Instance, out Dictionary<uint, Dictionary<uint, uint>>? killDict))
        {
            killDict.Remove(SERVICES.ClientState.TerritoryType);
            if (killDict.Count == 0) InstancedKillGatherCounts.Remove(SERVICES.ClientState.Instance);
        }
        if (InstancedFateSuccessTimes.TryGetValue(SERVICES.ClientState.Instance, out Dictionary<uint, Dictionary<uint, DateTime>>? fateDict))
        {
            fateDict.Remove(SERVICES.ClientState.TerritoryType);
            if (fateDict.Count == 0) InstancedFateSuccessTimes.Remove(SERVICES.ClientState.Instance);
        }
        Save();
    }

    public void IncrementDiscard()
    {
        DiscardCount++;
        Save();
    }

    public uint GetDiscardCount() => DiscardCount;
}