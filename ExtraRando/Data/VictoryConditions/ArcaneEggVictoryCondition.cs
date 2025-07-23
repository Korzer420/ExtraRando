using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using KorzUtils.Helper;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using RandomizerMod;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtraRando.Data.VictoryConditions;

public class ArcaneEggVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(4, Math.Max(setAmount, 0));

    public string GetMenuName() => "Arcane Eggs";

    public string PrepareLogic(LogicManagerBuilder logicBuilder) => "ARCANEEGGS";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    public string GetHintText()
    {
        Dictionary<string, int> leftItems = [];
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;
            if (item.name == ItemNames.Arcane_Egg)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                if (!leftItems.ContainsKey(area))
                    leftItems.Add(area, 0);
                leftItems[area]++;
            }
        }
        if (leftItems.Count == 0)
            return null;
        string text = "These eggs should be hidden in:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.trinket4))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
