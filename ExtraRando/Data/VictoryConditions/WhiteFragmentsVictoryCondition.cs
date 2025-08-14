using ItemChanger;
using RandomizerCore.Logic;
using System;
using System.Collections.Generic;
using Modding;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;

namespace ExtraRando.Data.VictoryConditions;

internal class WhiteFragmentsVictoryCondition : IVictoryCondition
{
    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(3, Math.Max(0, setAmount));

    private static readonly HashSet<string> CHARM_ITEMS = [ItemNames.Queen_Fragment, ItemNames.King_Fragment, ItemNames.Void_Heart];

    public string GetHintText() => this.GenerateHintText("The parts of the king's charm can be found at:", item => CHARM_ITEMS.Contains(item.name));

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
            this.CheckForEnding();
        }
        return orig;
    }
}
