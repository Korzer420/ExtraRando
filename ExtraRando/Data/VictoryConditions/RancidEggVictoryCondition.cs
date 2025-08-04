using ItemChanger.Internal;
using ItemChanger;
using RandomizerCore.Logic;
using System;
using System.Collections.Generic;
using RandomizerMod.Extensions;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using KorzUtils.Helper;
using System.Linq;
using Modding;

namespace ExtraRando.Data.VictoryConditions;

public class RancidEggVictoryCondition : IVictoryCondition
{
    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(18, Math.Max(0, setAmount));

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
        string text = "A stench emerges from:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

    public string GetMenuName() => "Rancid Eggs";

    public string PrepareLogic(LogicManagerBuilder logicBuilder) => $"RANCIDEGGS>{RequiredAmount - 1}";

    public void StartListening()
    {
        ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;
        Events.AddFsmEdit(new("Alive_Tuk", "Steel Soul"), PreventNormalTuk);
        Events.AddFsmEdit(new("Dead_Tuk", "Steel Soul"), ForceDeadTuk);
    }

    public void StopListening()
    {
        ModHooks.SetPlayerIntHook -= ModHooks_SetPlayerIntHook;
        Events.RemoveFsmEdit(new("Alive_Tuk", "Steel Soul"), PreventNormalTuk);
        Events.RemoveFsmEdit(new("Dead_Tuk", "Steel Soul"), ForceDeadTuk);
    }

    private int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.rancidEggs))
            if (PDHelper.RancidEggs < orig)
            {
                CurrentAmount += orig - PDHelper.RancidEggs;
                ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
            }
        return orig;
    }

    private void PreventNormalTuk(PlayMakerFSM fsm)
    {
        if (Ref.Settings.GetPlacements().Any(x => x.Name == LocationNames.Rancid_Egg_Tuk_Defenders_Crest))
            return;
        fsm.GetState("Pause").AdjustTransitions("Inactive");
    }

    private void ForceDeadTuk(PlayMakerFSM fsm)
    {
        if (Ref.Settings.GetPlacements().Any(x => x.Name == LocationNames.Rancid_Egg_Tuk_Defenders_Crest))
            return;
        fsm.GetState("Check").ClearTransitions();
    }
}
