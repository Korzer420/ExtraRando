using ItemChanger.Items;
using RandomizerCore.Logic;
using System;

namespace ExtraRando.Data.VictoryConditions;

public class GrubsVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(46, Math.Max(0, setAmount));

    public string GetMenuName() => "Grubs";

    public string PrepareLogic(LogicManagerBuilder builder) => $"GRUBS>{RequiredAmount - 1}";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    public string GetHintText() => this.GenerateHintText("Sad noises emit from:", item => item is GrubItem);

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.grubsCollected))
        {
            CurrentAmount++;
            this.CheckForEnding();
        }
    }
}
