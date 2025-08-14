using Modding;
using RandomizerCore.Logic;
using System;

namespace ExtraRando.Data.VictoryConditions;

internal class CharmVictoryCondition : IVictoryCondition
{
    #region Interface

    /// <inheritdoc/>
    public int CurrentAmount { get; set; }

    /// <inheritdoc/>
    public int RequiredAmount { get; set; }

    /// <inheritdoc/>
    public int ClampAvailableRange(int setAmount) => Math.Min(40, Math.Max(0, setAmount));

    /// <inheritdoc/>
    public string GetMenuName() => "Charms";

    public string PrepareLogic(LogicManagerBuilder builder) => $"CHARMS>{RequiredAmount - 1}";

    /// <inheritdoc/>
    public void StartListening() => ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;

    /// <inheritdoc/>
    public void StopListening() => ModHooks.SetPlayerBoolHook -= ModHooks_SetPlayerBoolHook;

    public string GetHintText()
    {
        if (!RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Charms)
            return null;

        return this.GenerateHintText("Ooooooooooooohhhhhhh, my lovely charms should be at:", item => item is ItemChanger.Items.CharmItem);
    }

    #endregion

    #region Eventhandler

    private bool ModHooks_SetPlayerBoolHook(string name, bool orig)
    {
        if (name.StartsWith("gotCharm_") && orig && !PlayerData.instance.GetBool(name))
        {
            CurrentAmount++;
            this.CheckForEnding();
        }
        return orig;
    }

    #endregion
}
