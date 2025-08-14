using ItemChanger;
using System;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using RandomizerCore.Logic;

namespace ExtraRando.Data.VictoryConditions;

internal class KingsIdolVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(8, Math.Max(0, setAmount));

    public string GetHintText() => this.GenerateHintText("The idols can be found at:", item => item.name == ItemNames.Kings_Idol);

    public string PrepareLogic(LogicManagerBuilder builder) => $"KINGSIDOLS>{RequiredAmount - 1}";

    public string GetMenuName() => "King's Idol";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.trinket3))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
