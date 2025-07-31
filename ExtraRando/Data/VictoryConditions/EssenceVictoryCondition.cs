using ExtraRando.Enums;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Items;
using Modding;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;

namespace ExtraRando.Data.VictoryConditions;

public class EssenceVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Max(0, Math.Min(setAmount, 2400));

    public string PrepareLogic(LogicManagerBuilder builder) => $"ESSENCE>{RequiredAmount - 1}";

    public string GetMenuName() => "Essence";

    public void StartListening() => ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;

    public void StopListening() => ModHooks.SetPlayerIntHook -= ModHooks_SetPlayerIntHook;

    public string GetHintText()
    {
        Dictionary<string, int> leftItems = [];
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;
            if (item is EssenceItem essence)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                if (!leftItems.ContainsKey(area))
                    leftItems.Add(area, 0);
                leftItems[area] += essence.amount;
            }
        }
        if (leftItems.Count == 0)
            return null;
        string text = "Old memories lie at:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

    #endregion

    private int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.dreamOrbs))
        {
            CurrentAmount = orig;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
        return orig;
    }
}
