using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Tags;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtraRando.Data;

/// <summary>
/// Represents a condition used in the <see cref="VictoryModule"/>.
/// <para/>To initiate a victory check, call <see cref="VictoryModule.CheckForFinish"/>.
/// </summary>
public interface IVictoryCondition
{
    /// <summary>
    /// Gets or sets the current amount.
    /// <para/>If this is bigger than <see cref="RequiredAmount"/>, the condition is considered met.
    /// </summary>
    public int CurrentAmount { get; set; }

    /// <summary>
    /// Gets or sets the required amount.
    /// </summary>
    public int RequiredAmount { get; set; }

    /// <summary>
    /// Gets the name that the mod should display in the victory settings page.
    /// </summary>
    public string GetMenuName();

    /// <summary>
    /// Gets the logic that is used for black egg temple access (e.g. for world sense).
    /// <para/>This method is called when the victory condition should be used and should serve as a setup opportunity for the logic in question.
    /// <para/>Keep in mind that you need to handle the required amount yourself!
    /// </summary>
    public string PrepareLogic(LogicManagerBuilder logicBuilder);

    /// <summary>
    /// Gets the text displayed as a hint on the hint tablet.
    /// <para/>If this condition is relying on ItemChanger items, you might wanna fetch the area and provide that as a hint.
    /// <para/>For example: "3 in Crystal Peak, 2 in Crossroads".
    /// </summary>
    public string GetHintText();

    /// <summary>
    /// Clamps the set menu number in between the allowed value.
    /// Return a valid value.
    /// </summary>
    /// <param name="setAmount">The input of the user which should be verified.</param>
    public int ClampAvailableRange(int setAmount);

    /// <summary>
    /// Starts listening to track possible changes.
    /// <para/>This is called when the <see cref="VictoryModule"/> is initialized.
    /// </summary>
    public void StartListening();

    /// <summary>
    /// Gives an opportunity to remove set hooks.
    /// <para/>This is called when the <see cref="VictoryModule"/> is unloaded.
    /// </summary>
    public void StopListening();
}

/// <summary>
/// Helper methods for implementing IVictoryConditions.
/// </summary>
public static class IVictoryConditionHelpers
{
    private static string GetDisplaySource(AbstractPlacement placement)
    {
        foreach (var tag in placement.GetPlacementAndLocationTags().OfType<IInteropTag>())
        {
            if (tag.Message != "RecentItems") continue;
            if (tag.TryGetProperty("DisplaySource", out string value)) return value;
        }

        return null;
    }

    /// <summary>
    /// List all areas where missing items relevant to the victory condition may be found.
    /// </summary>
    /// <param name="self">The invoking victory condition.</param>
    /// <param name="desc">The header to display before listing relevant areas.</param>
    /// <param name="counter">Returns the total count towards the victory condition produced by this item.</param>
    /// <returns>Suitable string for GetHintText().</returns>
    public static string GenerateHintText(this IVictoryCondition self, string desc, Func<AbstractItem, int> counter)
    {
        Dictionary<string, int> leftItems = [];
        foreach (var placement in Ref.Settings.Placements.Values)
        {
            string source = null;
            foreach (var item in placement.Items)
            {
                if (item.IsObtained())
                    continue;

                int count = counter(item);
                if (count > 0)
                {
                    source ??= GetDisplaySource(placement) ?? placement.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    if (!leftItems.ContainsKey(source))
                        leftItems.Add(source, count);
                    else
                        leftItems[source] += count;
                }
            }
        }
        if (leftItems.Count == 0)
            return null;

        return string.Join("<br>", leftItems.OrderByDescending(e => e.Value).ThenBy(e => e.Key).Select(e => $"{e.Value} in {e.Key}").Prepend(desc));
    }

    /// <summary>
    /// A simplified helper where matching items always count as 1 towards the victory condition.
    /// </summary>
    /// <param name="self">The invoking victory condition.</param>
    /// <param name="desc">The header to display before listing relevant areas.</param>
    /// <param name="filter">Returns true if the item contributes 1 towards the victory condition.</param>
    /// <returns>Suitable string for GetHintText().</returns>
    public static string GenerateHintText(this IVictoryCondition self, string desc, Func<AbstractItem, bool> filter) => self.GenerateHintText(desc, item => filter(item) ? 1 : 0);
}
