using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger.Internal;
using ItemChanger;
using System.Collections.Generic;
using System;
using RandomizerMod.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace ExtraRando.Data.VictoryConditions;

public class RelicVictoryCondition : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(43, Math.Max(0, setAmount));

    public string GetHintText()
    {
        Dictionary<string, int> leftItems = [];
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;
            if (item.name == ItemNames.Kings_Idol || item.name == ItemNames.Arcane_Egg
                || item.name == ItemNames.Hallownest_Seal || item.name == ItemNames.Wanderers_Journal)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                if (!leftItems.ContainsKey(area))
                    leftItems.Add(area, 0);
                leftItems[area]++;
            }
        }
        if (leftItems.Count == 0)
            return null;
        string text = "Bring me the relics you can find at:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

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
        return "RELICS";
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
