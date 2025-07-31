using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using KorzUtils.Helper;
using RandomizerCore.Logic;
using System;

namespace ExtraRando.Data.VictoryConditions;

internal class MillibelleVictoryCondition : IVictoryCondition
{
    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(4500, Math.Max(0, setAmount));

    public string GetHintText()
    {
        int leftGeo = RequiredAmount - CurrentAmount;
        return $"{leftGeo} geo is still left to invest.";
    }

    public string GetMenuName() => "Millibelle Deposit";

    public string PrepareLogic(LogicManagerBuilder logicBuilder) => "Fungus3_35[right1]/ + Can_Replenish_Geo";

    public void StartListening()
    {
        Events.AddFsmEdit(new("Banker", "Conversation Control"), PreventThievery);
    }

    public void StopListening()
    {
        Events.RemoveFsmEdit(new("Banker", "Conversation Control"), PreventThievery);
    }

    private void PreventThievery(PlayMakerFSM fsm)
    {
        if (ItemChangerMod.Modules.Get<VictoryModule>().Triggered)
            return;
        fsm.GetState("Init").AdjustTransitions("Grimmchild?");
        fsm.GetState("Farewell").AddActions(() =>
        {
            CurrentAmount = PDHelper.BankerBalance;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        });

    }
}
