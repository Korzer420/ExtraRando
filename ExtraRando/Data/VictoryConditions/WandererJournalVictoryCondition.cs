using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger.Internal;
using ItemChanger;
using System.Collections.Generic;
using System;
using RandomizerMod.Extensions;
using RandomizerCore.Logic;

namespace ExtraRando.Data.VictoryConditions;

public class WandererJournalVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(14, Math.Max(0, setAmount));

    public string GetHintText()
    {
        Dictionary<string, int> leftItems = [];
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;
            if (item.name == ItemNames.Wanderers_Journal)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                if (!leftItems.ContainsKey(area))
                    leftItems.Add(area, 0);
                leftItems[area]++;
            }
        }
        if (leftItems.Count == 0)
            return null;
        string text = "The wanderer journal lie at:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

    public string PrepareLogic(LogicManagerBuilder builder) => "WANDERERSJOURNALS";

    public string GetMenuName() => "Wanderer's Journal";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.trinket1))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
