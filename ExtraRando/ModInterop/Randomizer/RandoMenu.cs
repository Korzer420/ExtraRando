using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;

namespace ExtraRando.ModInterop.Randomizer;

internal class RandoMenu
{
    #region Members

    private static RandoMenu _instance;
    private MenuPage _connectionPage;
    private MenuElementFactory<RandoSettings> _elementFactory;

    #endregion

    #region Properties

    public static RandoMenu Instance => _instance ??= new();

    #endregion

    #region Event handler

    private void ConstructMenu(MenuPage previousPage)
    {
        // Generate pages and setting elements
        _connectionPage = new("Extra Rando", previousPage);
        _elementFactory = new(_connectionPage, ExtraRando.Instance.Settings);
        new VerticalItemPanel(_connectionPage, new(0f, 400f), 100f, true, _elementFactory.Elements);
    }

    private bool HandleButton(MenuPage previousPage, out SmallButton connectionButton)
    {
        SmallButton button = new(previousPage, "Extra Rando");
        button.AddHideAndShowEvent(previousPage, _connectionPage);
        _connectionPage.BeforeGoBack += () => button.Text.color = !ExtraRando.Instance.Settings.Enabled ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;
        button.Text.color = !ExtraRando.Instance.Settings.Enabled ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;
        connectionButton = button;
        return true;
    }

    #endregion

    #region Methods

    internal static void Initialize()
    {
        RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
        MenuChangerMod.OnExitMainMenu += () => _instance = null;
    }

    internal void PassSettings(RandoSettings randoSettings)
    {
        if (randoSettings == null)
            _elementFactory.ElementLookup[nameof(ExtraRando.Instance.Settings.Enabled)].SetValue(false);
        else
            _elementFactory.SetMenuValues(randoSettings);
    }

    #endregion
}
