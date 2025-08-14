using ItemChanger.Items;
using Modding;
using RandomizerCore.Logic;
using System;

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

    public string GetHintText() => this.GenerateHintText("Old memories lie at:", item => (item is EssenceItem essence) ? essence.amount : 0);

    #endregion

    private int ModHooks_SetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.dreamOrbs))
        {
            CurrentAmount = orig;
            this.CheckForEnding();
        }
        return orig;
    }
}
