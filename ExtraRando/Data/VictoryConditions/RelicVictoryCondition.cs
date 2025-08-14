using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using System.Collections.Generic;
using System;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace ExtraRando.Data.VictoryConditions;

public class RelicVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(43, Math.Max(0, setAmount));

    private static readonly HashSet<string> RELIC_NAMES = [ItemNames.Wanderers_Journal, ItemNames.Hallownest_Seal, ItemNames.Kings_Idol, ItemNames.Arcane_Egg];

    public string GetHintText() => this.GenerateHintText("Bring me the relics you can find at:", item => RELIC_NAMES.Contains(item.name));

    public string PrepareLogic(LogicManagerBuilder builder)
    {
        // We need the IsTerm check in case the term already exists with the same type.
        Term term = builder.IsTerm("RELICS") 
            ? builder.GetTerm("RELICS")
            : builder.GetOrAddTerm("RELICS", TermType.Int);
        builder.AddItem(new MultiItem(ItemNames.Wanderers_Journal,
        [
            new(term, 1),
            new(builder.GetTerm("WANDERERSJOURNALS"), 1)
        ]));
        builder.AddItem(new MultiItem(ItemNames.Hallownest_Seal,
        [
            new(term, 1),
            new(builder.GetTerm("HALLOWNESTSEALS"), 1)
        ]));
        builder.AddItem(new MultiItem(ItemNames.Kings_Idol,
        [
            new(term, 1),
            new(builder.GetTerm("KINGSIDOLS"), 1)
        ]));
        builder.AddItem(new MultiItem(ItemNames.Arcane_Egg,
        [
            new(term, 1),
            new(builder.GetTerm("ARCANEEGGS"), 1)
        ]));
        return $"RELICS>{RequiredAmount - 1}";
    }

    public string GetMenuName() => "Relics";

    public void StartListening() => On.PlayerData.IncrementInt += PlayerData_IncrementInt;

    public void StopListening() => On.PlayerData.IncrementInt -= PlayerData_IncrementInt;

    #endregion

    private void PlayerData_IncrementInt(On.PlayerData.orig_IncrementInt orig, PlayerData self, string intName)
    {
        orig(self, intName);
        if (intName.StartsWith("trinket"))
        {
            CurrentAmount++;
            ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
        }
    }
}
