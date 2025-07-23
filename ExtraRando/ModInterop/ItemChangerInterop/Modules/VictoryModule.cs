using ExtraRando.Data;
using ExtraRando.Data.VictoryConditions;
using ExtraRando.Enums;
using GlobalEnums;
using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.Util;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BossStatueCompletionStates;
using UnityEngine.SceneManagement;

namespace ExtraRando.ModInterop.ItemChangerInterop.Modules;

public class VictoryModule : Module
{
    #region Static

    public static List<IVictoryCondition> AvailableConditions { get; set; } = [];

    public static event Action<List<IVictoryCondition>> RequestConditions;

    #endregion

    #region Properties
    
    public bool WarpToCredits { get; set; }

    public List<IVictoryCondition> ActiveConditions { get; set; } = [];

    public bool CombineCondition { get; set; }

    public bool Triggered { get; set; }

    #endregion

    public override void Initialize()
    {
        foreach (IVictoryCondition condition in ActiveConditions)
            condition.StartListening();
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter += CheckBlackEggCondition;
        Events.AddSceneChangeEdit("Room_temple", VictoryHints);
        // Hint tablet at "Room_temple" 13.35f, 3.4
    }

    public override void Unload()
    {
        foreach (IVictoryCondition condition in ActiveConditions)
            condition.StopListening();
        On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.OnEnter -= CheckBlackEggCondition;
        Events.RemoveSceneChangeEdit("Room_temple", VictoryHints);
    }

    public void CheckForFinish()
    {
        if (Triggered)
            return;

        Triggered = CombineCondition;
        foreach (IVictoryCondition condition in ActiveConditions)
            if (condition.CurrentAmount < condition.RequiredAmount)
            {
                if (CombineCondition)
                {
                    Triggered = false;
                    return;
                }
            }
            else if (!CombineCondition)
            {
                Triggered = true;
                break;
            }
        if (Triggered)
        {
            if (WarpToCredits)
                GameManager.instance.StartCoroutine(MoveToScene("End_Game_Completion"));
            else
                GameHelper.DisplayMessage("You may now enter the black egg temple...");
        }
    }

    private IEnumerator MoveToScene(string scene)
    {
        yield return new WaitUntil(() => HeroController.instance.acceptingInput);
        HeroController.instance.RelinquishControl();
        ModHooks.GetPlayerBoolHook += PreventDamage;
        GameManager.instance.cameraCtrl.FadeOut(CameraFadeType.LEVEL_TRANSITION);
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.ChangeToScene(scene, scene == "End_Game_Completion" ? "door1" : "left1", 0);
        yield return new WaitWhile(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != scene);
        ModHooks.GetPlayerBoolHook -= PreventDamage;
    }

    private bool PreventDamage(string name, bool orig)
    {
        if (name == nameof(PlayerData.instance.disablePause) || name == nameof(PlayerData.instance.isInvincible))
            return true;
        return orig;
    }

    internal static void LoadConditions()
    {
        AvailableConditions.Clear();
        AvailableConditions.Add(new CharmVictoryCondition());
        AvailableConditions.Add(new DreamerVictoryCondition());
        AvailableConditions.Add(new GrubsVictoryCondition());
        AvailableConditions.Add(new EssenceVictoryCondition());
        AvailableConditions.Add(new WandererJournalVictoryCondition());
        AvailableConditions.Add(new HallownestSealVictoryCondition());
        AvailableConditions.Add(new KingsIdolVictoryCondition());
        AvailableConditions.Add(new ArcaneEggVictoryCondition());
        AvailableConditions.Add(new RelicVictoryCondition());
        AvailableConditions.Add(new ButtVictoryCondition());
        RequestConditions?.Invoke(AvailableConditions);

        foreach (var item in AvailableConditions)
        {
            string conditionName = item.GetType().Name;
            if (!ExtraRando.Instance.Settings.VictoryConditions.ContainsKey(conditionName))
                ExtraRando.Instance.Settings.VictoryConditions.Add(conditionName, 0);
        }
    }

    private void CheckBlackEggCondition(On.HutongGames.PlayMaker.Actions.GetPlayerDataInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.GetPlayerDataInt self)
    {
        orig(self);
        if (self.IsCorrectContext("Control", "Final Boss Door", "Init"))
            self.storeValue.Value = Triggered ? 3 : 0;
    }

    #region Hint Handling

    private void VictoryHints(Scene scene)
    {
        GameObject tablet = TabletUtility.MakeNewTablet("Victory tablet", () =>
        {
            string hintText = null;
            foreach (var condition in ActiveConditions)
            {
                if (condition.CurrentAmount >= condition.RequiredAmount)
                    continue;
                string conditionHint = condition.GetHintText();
                if (conditionHint == null)
                    continue;
                hintText += $"<page>{conditionHint}";
            }
            return hintText?.Substring("<page>".Length) ?? "This tablet is filled with indecipherable scribbles.";
        });
        tablet.transform.localPosition = new(18f,3.7f, 1.92f);
        tablet.SetActive(true);
    }

    #endregion
}
