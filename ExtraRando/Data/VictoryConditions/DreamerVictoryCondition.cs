using ItemChanger.Items;
using RandomizerCore.Logic;
using System;

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
        if (!RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Dreamers)
            return null;

        return this.GenerateHintText("The guardians watch at:", item => item is DreamerItem);
    }

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.guardiansDefeated))
        {
            CurrentAmount++;
            this.CheckForEnding();
        }
    }
}
