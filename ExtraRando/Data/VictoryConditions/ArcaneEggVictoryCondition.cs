using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using RandomizerCore.Logic;
using System;

namespace ExtraRando.Data.VictoryConditions;

public class ArcaneEggVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(4, Math.Max(setAmount, 0));

    public string GetMenuName() => "Arcane Eggs";

    public string PrepareLogic(LogicManagerBuilder logicBuilder) => $"ARCANEEGGS>{RequiredAmount - 1}";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    public string GetHintText() => this.GenerateHintText("These eggs should be hidden in:", item => item.name == ItemNames.Arcane_Egg);

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.trinket4))
        {
            CurrentAmount++;
            this.CheckForEnding();
        }
    }
}
