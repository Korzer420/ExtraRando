using ExtraRando.Data.VictoryConditions;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using System.Linq;

namespace ExtraRando.ModInterop.ItemChangerInterop;

public class ButtItem : AbstractItem
{
    #region Methods

    public override void GiveImmediate(GiveInfo info)
    {
        SoundManager manager = new(typeof(ExtraRando).Assembly, "ExtraRando.Resources.Sounds.");
        manager.PlayClipAtPoint("Smack_"+UnityEngine.Random.Range(1, 5), HeroController.instance.transform.position);
        if (ItemChangerMod.Modules.Get<VictoryModule>() is VictoryModule module 
            && module.ActiveConditions.FirstOrDefault(x => x.GetType() == typeof(ButtVictoryCondition)) is ButtVictoryCondition condition)
        {
            condition.CurrentAmount++;
            module.CheckForFinish();
        }
    }

    #endregion
}
