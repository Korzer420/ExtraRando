using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Tags;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtraRando.Data;

/// <summary>
/// Provides extension methods for <see cref="IVictoryCondition"/>.
/// </summary>
public static class IVictoryConditionExtensions
{
    #region Public Methods

    /// <summary>
    /// List all areas where missing items relevant to the victory condition may be found.
    /// </summary>
    /// <param name="self">The invoking victory condition.</param>
    /// <param name="descriptionHeader">The header to display before listing relevant areas.</param>
    /// <param name="counter">Returns the total count towards the victory condition produced by this item.</param>
    /// <returns>Suitable string for GetHintText().</returns>
    public static string GenerateHintText(this IVictoryCondition self,
        string descriptionHeader,
        Func<AbstractItem, int> counter)
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

        return string.Join("<br>", leftItems.OrderByDescending(e => e.Value)
            .ThenBy(e => e.Key)
            .Select(e => $"{e.Value} in {e.Key}")
            .Prepend(descriptionHeader));
    }

    /// <summary>
    /// A simplified helper where matching items always count as 1 towards the victory condition.
    /// </summary>
    /// <param name="self">The invoking victory condition.</param>
    /// <param name="desc">The header to display before listing relevant areas.</param>
    /// <param name="filter">Returns true if the item contributes 1 towards the victory condition.</param>
    /// <returns>Suitable string for GetHintText().</returns>
    public static string GenerateHintText(this IVictoryCondition self,
        string desc,
        Func<AbstractItem, bool> filter) => self.GenerateHintText(desc, item => filter(item) ? 1 : 0);
    
    /// <summary>
    /// Initiates the check for triggering the "ending" (which either opens black egg temple or warp the player to the credit).
    /// If the ending has been triggered already, this method does nothing.
    /// </summary>
    public static void CheckForEnding(this IVictoryCondition self) => ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();

    #endregion

    #region Private Methods

    /// <summary>
    /// Search through the tags of a placement to find the correct location.
    /// </summary>
    /// <param name="placement">The placement to search through.</param>
    private static string GetDisplaySource(AbstractPlacement placement)
    {
        foreach (var tag in placement.GetPlacementAndLocationTags().OfType<IInteropTag>())
        {
            if (tag.Message != "RecentItems")
                continue;
            if (tag.TryGetProperty("DisplaySource", out string value))
                return value;
        }
        return null;
    } 

    #endregion
}
