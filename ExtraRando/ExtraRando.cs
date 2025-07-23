using ExtraRando.ModInterop.ItemChangerInterop;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ExtraRando.ModInterop.Randomizer;
using ExtraRando.SaveManagement;
using Modding;

namespace ExtraRando;

public class ExtraRando : Mod, IGlobalSettings<GlobalSaveSettings>
{
    #region Constructors

    public ExtraRando() => Instance = this;

    #endregion

    #region Properties

    public override string GetVersion() => "0.7.0.0";

    public RandoSettings Settings { get; set; } = new();

    public static ExtraRando Instance { get; set; }

    #endregion

    #region Eventhandler

    #endregion

    #region Methods

    public override void Initialize()
    {
        ItemManager.Initialize();
        RandoInterop.Initialize();
    }

    public void OnLoadGlobal(GlobalSaveSettings saveSettings)
    {
        if (saveSettings is not null && saveSettings.Settings is not null)
            Settings = saveSettings.Settings;
    }

    public GlobalSaveSettings OnSaveGlobal() => new() { Settings = Settings };

    #endregion
}