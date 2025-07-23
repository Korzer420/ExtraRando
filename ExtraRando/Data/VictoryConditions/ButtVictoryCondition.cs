using ItemChanger.Internal;
using ItemChanger;
using System;
using ExtraRando.ModInterop.ItemChangerInterop;
using RandomizerMod.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;

namespace ExtraRando.Data.VictoryConditions;

public class ButtVictoryCondition : IVictoryCondition
{
    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount)
    {
        if (ExtraRando.Instance.Settings.RandomizeButt)
            return Math.Min(1, Math.Max(0, setAmount));
        return 0;
    }

    public string GetHintText()
    {
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;
            if (item is ButtItem)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                return "Butt supremacy can be found in " + area;
            }
        }
        return null;
    }

    public string PrepareLogic(LogicManagerBuilder builder)
    {
        // We need the IsTerm check in case the term already exists with the same type.
        Term term = builder.IsTerm("BUTT")
            ? builder.GetTerm("BUTT")
            : builder.GetOrAddTerm("BUTT");
        builder.AddItem(new SingleItem(ItemManager.Bardoon_Butt_Smack, new(term, 1)));
        return "BUTT";
    }

    public string GetMenuName() => "Bardoon's Butt";

    // Logic is handled via ButtItem
    
    public void StartListening() { }

    public void StopListening() { }
}
