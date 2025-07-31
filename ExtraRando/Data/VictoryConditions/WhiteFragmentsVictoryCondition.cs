using ItemChanger.Internal;
using ItemChanger;
using RandomizerCore.Logic;
using System;
using System.Collections.Generic;
using RandomizerMod.Extensions;
using KorzUtils.Helper;
using Modding;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;

namespace ExtraRando.Data.VictoryConditions;

internal class WhiteFragmentsVictoryCondition : IVictoryCondition
{
    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(3, Math.Max(0, setAmount));

    public string GetHintText()
    {
        Dictionary<string, int> leftItems = [];
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;
            if (item.name == ItemNames.Queen_Fragment || item.name == ItemNames.King_Fragment
                || item.name == ItemNames.Void_Heart)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                if (!leftItems.ContainsKey(area))
                    leftItems.Add(area, 0);
                leftItems[area]++;
            }
        }
        if (leftItems.Count == 0)
            return null;
        string text = "The parts of the kings charm can be found at:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

    public string GetMenuName() => "White Fragments";

    public string PrepareLogic(LogicManagerBuilder logicBuilder)
    {
        if (RequiredAmount > 2)
            return "VOIDHEART";
        else
            return $"WHITEFRAGMENT>{RequiredAmount - 1}";
    }

    public void StartListening() => ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;

    public void StopListening() => ModHooks.SetPlayerIntHook -= ModHooks_SetPlayerIntHook;

    private int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.royalCharmState))
        {
            if (orig == 1 || orig == 2)
                CurrentAmount = 1;
            else if (orig > 2)
                CurrentAmount = orig - 1;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
        return orig;
    }
}
