using ExtraRando.Data;
using ExtraRando.Enums;
using MenuChanger.Attributes;
using System.Collections.Generic;

namespace ExtraRando.ModInterop.Randomizer;

public class RandoSettings
{
    public bool Enabled { get; set; }

    public bool SplitShadeCloak { get; set; }

    public bool SplitFireball { get; set; }

    public bool UseKeyring { get; set; }

    public bool ScarceItemPool { get; set; }

    public bool RandomizeHotSprings { get; set; }

    public bool AddHintMarkers { get; set; }

    public bool RandomizeColoAccess { get; set; }

    public bool RandomizePantheonAccess { get; set; }

    public bool RandomizeButt { get; set; }

    public bool RandomizeAwfulLocations { get; set; }

    public bool BlockEarlyGameStags { get; set; }

    public bool NoLogic { get; set; }

    public int JunkItemHints { get; set; }

    public int PotentialItemHints { get; set; }

    public int UsefulItemHints { get; set; }

    public int RandomLocationHints { get; set; }

    public bool AddFixedHints { get; set; }

    public bool EnforceJunkLocations { get; set; }

    /// <summary>
    /// Gets or sets all victory conditions that are available.
    /// <para/>If the value is not set to 0, the condition is used.
    /// </summary>
    [MenuIgnore]
    public Dictionary<string,int> VictoryConditions { get; set; } = [];

    [MenuIgnore]
    public bool UseVictoryConditions { get; set; }

    [MenuIgnore]
    public VictoryConditionHandling ConditionHandling { get; set; }

    [MenuIgnore]
    public bool WarpToCredits { get; set; }
}
