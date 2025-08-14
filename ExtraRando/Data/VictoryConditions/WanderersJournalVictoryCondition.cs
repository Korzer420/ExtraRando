using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger.Internal;
using ItemChanger;
using System;
using RandomizerCore.Logic;

namespace ExtraRando.Data.VictoryConditions;

public class WanderersJournalVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(14, Math.Max(0, setAmount));

    public string GetHintText() => this.GenerateHintText("The wanderer's journals lie at:", item => item.name == ItemNames.Wanderers_Journal);

    public string PrepareLogic(LogicManagerBuilder builder) => $"WANDERERSJOURNALS>{RequiredAmount - 1}";

    public string GetMenuName() => "Wanderer's Journal";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.trinket1))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
