using ItemChanger;
using ItemChanger.Locations;
using KorzUtils.Helper;
using System.Linq;
using UnityEngine.SceneManagement;

namespace ExtraRando.ModInterop.ItemChangerInterop;

public class SplitVengefulSpiritLocation : AutoLocation
{
    #region Eventhandler

    private void SetPlayerDataInt_OnEnter(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetPlayerDataInt self)
    {
        if (self.IsCorrectContext("Conversation Control", "Shaman Meeting", "Spell Appear"))
            ItemHelper.SpawnShiny(new(19.41f, 11.55f), Placement);
        orig(self);
    }

    public void OnEnterScene(Scene scene)
    {
        if (!Placement.AllObtained() && Placement.Items.All(x => x.WasEverObtained()))
            ItemHelper.SpawnShiny(new(19.41f, 11.55f), Placement);
    }

    #endregion

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit("Crossroads_ShamanTemple", OnEnterScene);
        On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += SetPlayerDataInt_OnEnter;
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit("Crossroads_ShamanTemple", OnEnterScene);
        On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter -= SetPlayerDataInt_OnEnter;
    }
}
