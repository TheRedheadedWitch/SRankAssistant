using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SRankAssistant;

internal static class FateTracker
{
    internal sealed class FateStatus
    {
        public ushort Count;
        public DateTime? LastSeen;
        public bool LastSuccess;
        public bool Active;
    }

    private static readonly Dictionary<uint, Dictionary<uint, Dictionary<ushort, FateStatus>>> InstancedFateStatuses = new();

    internal static void Initialize()
    {
        FateFailureTracker.EnterZone();
        SERVICES.Framework.Update += OnUpdate;
    }

    internal static void Dispose() => SERVICES.Framework.Update -= OnUpdate;

    private static Dictionary<ushort, FateStatus> GetCurrentFateStatuses()
    {
        if (!InstancedFateStatuses.TryGetValue(SERVICES.ClientState.Instance, out Dictionary<uint, Dictionary<ushort, FateStatus>>? instanceDict))
            InstancedFateStatuses[SERVICES.ClientState.Instance] = instanceDict = new Dictionary<uint, Dictionary<ushort, FateStatus>>();
        if (!instanceDict.TryGetValue(SERVICES.ClientState.TerritoryType, out Dictionary<ushort, FateStatus>? zoneDict))
            instanceDict[SERVICES.ClientState.TerritoryType] = zoneDict = new Dictionary<ushort, FateStatus>();
        return zoneDict;
    }

    internal static FateStatus? GetStatusForTarget(ushort targetId)
    {
        SRankCondition? condition = SRankData.GetCondition();
        if (condition == null || (condition.Type != SRankConditionType.Fate && condition.Type != SRankConditionType.FateTimer)) return null;
        bool isTarget = false;
        foreach ((uint _, uint tId) in condition.Targets)
            if (tId == targetId)
            {
                isTarget = true;
                break;
            }
        if (!isTarget) return null;
        if (GetCurrentFateStatuses().TryGetValue(targetId, out FateStatus? status))
            return status;
        return null;
    }

    private static void OnUpdate(IFramework framework)
    {
        SRankCondition? condition = SRankData.GetCondition();
        if (condition == null || condition.Type != SRankConditionType.Fate && condition.Type != SRankConditionType.FateTimer) return;
        Dictionary<ushort, FateStatus> currentStatuses = GetCurrentFateStatuses();
        HashSet<ushort> seenFates = new();
        foreach (IFate fate in SERVICES.Fates)
        {
            seenFates.Add(fate.FateId);
            if (!currentStatuses.TryGetValue(fate.FateId, out FateStatus? status))
                currentStatuses[fate.FateId] = status = new FateStatus { Active = true };
            if (fate.State == FateState.Running || fate.State == FateState.Preparation) status.Active = true;
            if ((fate.State == FateState.WaitingForEnd || fate.State == FateState.Ended) && status.Active)
            {
                status.Count++;
                status.LastSeen = DateTime.UtcNow;
                status.LastSuccess = true;
                status.Active = false;
                if (condition.Type == SRankConditionType.Fate)
                    foreach ((uint goal, uint targetId) in condition.Targets)
                        if (fate.FateId == targetId)
                        {
                            Globals.tracker.Increment(targetId);
                            LOG.Debug($"FATE ID {fate.FateId} completed in zone {SERVICES.ClientState.TerritoryType} (Instance {SERVICES.ClientState.Instance}) (Count {status.Count})");
                            break;
                        }
            }
            if (fate.State == FateState.Failed && status.Active)
            {
                status.Count = 0;
                status.LastSeen = DateTime.UtcNow;
                status.LastSuccess = false;
                status.Active = false;
                FateFailureTracker.RecordFailure();
                if (condition.Type == SRankConditionType.Fate)
                    foreach ((uint goal, uint targetId) in condition.Targets)
                        if (fate.FateId == targetId)
                        {
                            Globals.tracker.ResetZone();
                            LOG.Debug($"FATE ID {fate.FateId} failed in zone {SERVICES.ClientState.TerritoryType} (Instance {SERVICES.ClientState.Instance})");
                            break;
                        }
            }
        }
        foreach (ushort fateId in currentStatuses.Keys.ToList())
            if (!seenFates.Contains(fateId))
            {
                if (currentStatuses.TryGetValue(fateId, out FateStatus? status) && status.Active)
                {
                    status.Count = 0;
                    status.LastSeen = DateTime.UtcNow;
                    status.LastSuccess = false;
                    status.Active = false;
                    FateFailureTracker.RecordFailure();
                    if (condition?.Type == SRankConditionType.Fate)
                        foreach ((uint goal, uint targetId) in condition.Targets)
                            if (fateId == targetId)
                            {
                                Globals.tracker.ResetZone();
                                break;
                            }
                    LOG.Debug($"FATE {fateId} despawned uncompleted in zone {SERVICES.ClientState.TerritoryType} (Instance {SERVICES.ClientState.Instance})");
                }
                currentStatuses.Remove(fateId);
            }
    }
}

internal static class FateFailureTracker
{
    private static readonly Dictionary<uint, Dictionary<uint, DateTime>> LastFailure = new();
    private static readonly Dictionary<uint, Dictionary<uint, DateTime>> ZoneEntry = new();

    internal static void EnterZone()
    {
        if (!ZoneEntry.ContainsKey(SERVICES.ClientState.TerritoryType)) ZoneEntry[SERVICES.ClientState.TerritoryType] = new Dictionary<uint, DateTime>();
        ZoneEntry[SERVICES.ClientState.TerritoryType][SERVICES.ClientState.Instance] = DateTime.UtcNow;
    }

    internal static void RecordFailure()
    {
        if (!LastFailure.ContainsKey(SERVICES.ClientState.TerritoryType)) LastFailure[SERVICES.ClientState.TerritoryType] = new Dictionary<uint, DateTime>();
        LastFailure[SERVICES.ClientState.TerritoryType][SERVICES.ClientState.Instance] = DateTime.UtcNow;
        LOG.Debug($"FATE failure recorded in zone {SERVICES.ClientState.TerritoryType} (Instance {SERVICES.ClientState.Instance}) at {LastFailure[SERVICES.ClientState.TerritoryType][SERVICES.ClientState.Instance]}");
    }

    internal static TimeSpan TimeSinceFailureOrEntry()
    {
        if (LastFailure.TryGetValue(SERVICES.ClientState.TerritoryType, out Dictionary<uint, DateTime>? failDict) && failDict.TryGetValue(SERVICES.ClientState.Instance, out DateTime lastFail))
            return DateTime.UtcNow - lastFail;
        if (ZoneEntry.TryGetValue(SERVICES.ClientState.TerritoryType, out Dictionary<uint, DateTime>? entryDict) && entryDict.TryGetValue(SERVICES.ClientState.Instance, out DateTime entry))
            return DateTime.UtcNow - entry;
        return TimeSpan.Zero;
    }
}