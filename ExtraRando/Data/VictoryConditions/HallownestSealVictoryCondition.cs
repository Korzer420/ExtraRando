using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using System;
using RandomizerCore.Logic;

namespace ExtraRando.Data.VictoryConditions;

internal class HallownestSealVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(17, Math.Max(0, setAmount));

    public string GetHintText() => this.GenerateHintText("Seals can be located at:", item => item.name == ItemNames.Hallownest_Seal);

    public string PrepareLogic(LogicManagerBuilder builder) => $"HALLOWNESTSEALS>{RequiredAmount - 1}";

    public string GetMenuName() => "Hallownest Seals";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName == nameof(PlayerData.trinket2))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
