using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using KorzUtils.Helper;
using Modding;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;

namespace ExtraRando.Data.VictoryConditions;

internal class CharmVictoryCondition : IVictoryCondition
{
    #region Interface

    /// <inheritdoc/>
    public int CurrentAmount { get; set; }

    /// <inheritdoc/>
    public int RequiredAmount { get; set; }

    /// <inheritdoc/>
    public int ClampAvailableRange(int setAmount) => Math.Min(40, Math.Max(0, setAmount));

    /// <inheritdoc/>
    public string GetMenuName() => "Charms";

    public string PrepareLogic(LogicManagerBuilder builder) => "CHARMS";

    /// <inheritdoc/>
    public void StartListening() => ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;

    /// <inheritdoc/>
    public void StopListening() => ModHooks.SetPlayerBoolHook -= ModHooks_SetPlayerBoolHook;

    public string GetHintText()
    {
        if (RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Charms)
        {
            Dictionary<string, int> leftItems = [];
            foreach (AbstractItem item in Ref.Settings.GetItems())
            {
                if (item.IsObtained())
                    continue;
                if (item is ItemChanger.Items.CharmItem)
                {
                    string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    if (!leftItems.ContainsKey(area))
                        leftItems.Add(area, 0);
                    leftItems[area]++;
                }
            }
            if (leftItems.Count == 0)
                return null;
            string text = "Ooooooooooooohhhhhhh, my lovely charms should be at:";
            foreach (string item in leftItems.Keys)
                text += $"<br>{leftItems[item]} in {item}";
            return text;
        }
        return null;
    }

    #endregion

    #region Eventhandler

    private bool ModHooks_SetPlayerBoolHook(string name, bool orig)
    {
        if (name.StartsWith("gotCharm_") && orig)
        {
            if (!PlayerData.instance.GetBool(name))
            {
                CurrentAmount++;
                LogHelper.Write("Increase charm amount");
                ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
            }
        }
        return orig;
    }

    #endregion
}
