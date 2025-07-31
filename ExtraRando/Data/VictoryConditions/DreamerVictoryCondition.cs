using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Items;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;

namespace ExtraRando.Data.VictoryConditions;

public class DreamerVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Max(0, Math.Min(setAmount, 3));

    public string GetMenuName() => "Dreamer";

    public string PrepareLogic(LogicManagerBuilder builder) => $"DREAMER>{RequiredAmount - 1}";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    public string GetHintText()
    {
        if (RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Dreamers)
        {
            Dictionary<string, int> leftItems = [];
            foreach (AbstractItem item in Ref.Settings.GetItems())
            {
                if (item.IsObtained())
                    continue;
                if (item is DreamerItem)
                {
                    string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    if (!leftItems.ContainsKey(area))
                        leftItems.Add(area, 0);
                    leftItems[area]++;
                }
            }
            if (leftItems.Count == 0)
                return null;
            string text = "The guardians watch at:";
            foreach (string item in leftItems.Keys)
                text += $"<br>{leftItems[item]} in {item}";
            return text;
        }
        return null;
    }

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.guardiansDefeated))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
