using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Modules;
using ItemChanger.Util;
using KorzUtils.Helper;
using Modding;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExtraRando.ModInterop.ItemChangerInterop.Modules;

internal class HintModule : Module
{
    #region Members

    private static readonly Dictionary<string, (string, string)> _itemMapping = new()
    {
        { ItemNames.Mantis_Claw, ("MANTIS_PLAQUE_02", "Our secret tool placed in <color=#f9ff40>{0}</color>.") },
        { "Split_Claw", ("MANTIS_PLAQUE_02", "Our secret tools placed in <color=#f9ff40>{0}</color> and <color=#f9ff40>{1}</color>.") },
        { ItemNames.Mothwing_Cloak, ("HORNET_GREENPATH", "I already took their cloak to <color=#f9ff40>{0}</color>.") },
        { "Split_Cloak", ("HORNET_GREENPATH", "I already took their cloak to <color=#f9ff40>{0}</color> and <color=#f9ff40>{1}</color>.") },
        { ItemManager.Cloak, ("HORNET_GREENPATH", "I already took their cloak to <color=#f9ff40>{0}</color>.") },
        { ItemNames.Mark_of_Pride, ("MANTIS_PASSIVE_1", "Our symbol of respect at <color=#f9ff40>{0}</color>.") },
        { ItemNames.Vengeful_Spirit, ("SHAMAN_RETURN", "Well, have you found my gift at <color=#f9ff40>{0}</color> already?") },
        { ItemManager.Fireball_Spell, ("SHAMAN_RETURN", "Well, have you found my gift at <color=#f9ff40>{0}</color> already?") },
        { "Split_Fireball", ("SHAMAN_RETURN", "Well, have you found my gifts at <color=#f9ff40>{0}</color> and <color=#f9ff40>{1}</color> already?") },
        { ItemNames.Crystal_Heart, ("DREAM_MINES_ROBOT", "<color=#f9ff40>{0}</color>...CRUSH") },
        { "Split_Crystal_Heart", ("DREAM_MINES_ROBOT", "<color=#f9ff40>{0}</color>...CRUSH...<color=#f9ff40>{1}</color>") },
        { ItemNames.Dream_Nail, ("WITCH_GREET", "I still can sense a relic of my kind pulsing from <color=#f9ff40>{0}</color>.") },
        { ItemNames.Ismas_Tear, ("ISMA_DREAM", "...<color=#f9ff40>{0}</color>...") },
        { ItemNames.Lurien, ("LURIAN_JOURNAL", "I shall rest at <color=#f9ff40>{0}</color>.") },
        { ItemNames.Monomon, ("QUIRREL_TEACHER_HESITATE", "Although... I can hear her voice from <color=#f9ff40>{0}</color>. Quite odd, isn't it?") },
        { ItemNames.Herrah, ("SPIDER_MEET", "I don't have the luxury like the beast sleeping in <color=#f9ff40>{0}</color>.") },
        { ItemNames.Shopkeepers_Key, ("SLY_GENERIC", "Maybe you can even look for my key? I think I left it somewhere in <color=#f9ff40>{0}</color>.") },
        { ItemNames.Desolate_Dive, ("MAGE_LORD", "After discovering the magic from <color=#f9ff40>{0}</color>, I thought I finally could convince the king. How foolish...") },
        { "White_Fragments", ("QUEEN_MEET", "United from <color=#f9ff40>{0}</color> to <color=#f9ff40>{1}</color> no matter the way.") }
    };
    #endregion

    #region Properties

    public Dictionary<string, string> HintList { get; set; } = new();

    #endregion

    #region Methods

    private string BuildGrubText()
    {
        StringBuilder stringBuilder = new("I think my children should be at:");
        stringBuilder.Append("<br>");
        StringBuilder itemPreviewBuilder = new();
        Dictionary<string, int> grubLocations = [];
        foreach (var item in Ref.Settings.GetItems())
        {
            if (item.name != ItemNames.Grub || item.IsObtained())
                continue;
            string area = item.RandoLocation()?.LocationDef?.MapArea;
            if (string.IsNullOrEmpty(area))
                area = "An unknown place.";
            if (!grubLocations.ContainsKey(area))
                grubLocations.Add(area, 0);
            grubLocations[area]++;
        }
        foreach (string key in grubLocations.Keys)
        {
            itemPreviewBuilder.Append("<color=#f9ff40>");
            itemPreviewBuilder.Append(key);
            itemPreviewBuilder.Append(": ");
            itemPreviewBuilder.Append(grubLocations[key]);
            itemPreviewBuilder.Append("</color>");

            string text = itemPreviewBuilder.ToString();
            itemPreviewBuilder.Clear();

            stringBuilder.Append("<br>");
            stringBuilder.Append(text);
        }
        if (grubLocations.Count == 0)
            return "You have found all of them. Thank you!";
        
        return stringBuilder.ToString();
    }

    #endregion

    #region Eventhandler

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "SHAMAN_SUMMONED_1" || key == "SHAMAN_SUMMONED_2")
            orig += "\r\n" + HintList["SHAMAN_RETURN"];
        else if (HintList.ContainsKey(key))
            orig += "\r\n" + HintList[key];
        return orig;
    }

    private bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == "midwifeMet")
            return false;
        return orig;
    }

    #endregion

    #region Methods

    public override void Initialize()
    {
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        Events.AddFsmEdit(new("Dream Moth", "Conversation Control"), ForceSeerDialogue);
        Events.AddSceneChangeEdit("Crossroads_38", SpawnHintTablet);
    }

    private void SpawnHintTablet(Scene scene)
    {
        GameObject tablet = TabletUtility.MakeNewTablet("Hint tablet", BuildGrubText);
        tablet.transform.localPosition = new(25.85f, 3.6f, 2.5f);
        tablet.SetActive(true);
    }

    private void ForceSeerDialogue(PlayMakerFSM self) => self.GetState("Greet Choice")?.AdjustTransition("FINISHED", "Greet");
    
    public override void Unload()
    {
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook; 
        Events.RemoveFsmEdit(new("Dream Moth", "Conversation Control"), ForceSeerDialogue);
    }

    public void AddHints()
    {
        foreach (AbstractItem item in Ref.Settings.GetItems())
            // Try catch because duplicates cause issues.
            try
            {
                if (_itemMapping.ContainsKey(item.name))
                {
                    string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string value = string.Format(_itemMapping[item.name].Item2, area);
                    HintList.Add(_itemMapping[item.name].Item1, value);
                }
                else if (item.name == ItemNames.Left_Mantis_Claw)
                {
                    string leftClaw = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightClaw = Ref.Settings.GetItems().First(x => x.name == ItemNames.Right_Mantis_Claw).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Claw"].Item2, leftClaw, rightClaw);
                    HintList.Add(_itemMapping["Split_Claw"].Item1, value);
                }
                else if (item.name == ItemNames.Left_Mothwing_Cloak)
                {
                    string leftCloak = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightCloak = Ref.Settings.GetItems().First(x => x.name == ItemNames.Right_Mothwing_Cloak).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Cloak"].Item2, leftCloak, rightCloak);
                    HintList.Add(_itemMapping["Split_Cloak"].Item1, value);
                }
                else if (item.name == ItemManager.Left_Cloak)
                {
                    string leftCloak = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightCloak = Ref.Settings.GetItems().First(x => x.name == ItemManager.Right_Cloak).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Cloak"].Item2, leftCloak, rightCloak);
                    HintList.Add(_itemMapping["Split_Cloak"].Item1, value);
                }
                else if (item.name == ItemManager.Progressive_Left_Cloak)
                {
                    string leftCloak = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightCloak = Ref.Settings.GetItems().First(x => x.name == ItemManager.Progressive_Right_Cloak).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Cloak"].Item2, leftCloak, rightCloak);
                    HintList.Add(_itemMapping["Split_Cloak"].Item1, value);
                }
                else if (item.name == ItemManager.Left_Fireball)
                {
                    string leftFireball = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightFireball = Ref.Settings.GetItems().First(x => x.name == ItemManager.Right_Fireball).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Fireball"].Item2, leftFireball, rightFireball);
                    HintList.Add(_itemMapping["Split_Fireball"].Item1, value);
                }
                else if (item.name == ItemManager.Left_Vengeful_Spirit)
                {
                    string leftFireball = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightFireball = Ref.Settings.GetItems().First(x => x.name == ItemManager.Right_Vengeful_Spirit).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Fireball"].Item2, leftFireball, rightFireball);
                    HintList.Add(_itemMapping["Split_Fireball"].Item1, value);
                }
                else if (item.name == ItemNames.Left_Crystal_Heart)
                {
                    string leftCrystalHeart = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string rightCrystalHeart = Ref.Settings.GetItems().First(x => x.name == ItemNames.Right_Crystal_Heart).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["Split_Crystal_Heart"].Item2, leftCrystalHeart.ToUpper(), rightCrystalHeart.ToUpper());
                    HintList.Add(_itemMapping["Split_Crystal_Heart"].Item1, value);
                }
                else if (item.name == ItemNames.Queen_Fragment)
                {
                    string queenFragment = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                    string kingFragment = Ref.Settings.GetItems().First(x => x.name == ItemNames.King_Fragment).RandoLocation()?.LocationDef?.MapArea ?? "an unknown place";
                    string value = string.Format(_itemMapping["White_Fragments"].Item2, queenFragment, kingFragment);
                    HintList.Add(_itemMapping["White_Fragments"].Item1, value);
                }
            }
            catch (Exception)
            {
            }
    }

    #endregion
}
