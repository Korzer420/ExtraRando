using ExtraRando.Data;
using ExtraRando.Enums;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using KorzUtils.Helper;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using System.Collections.Generic;
using System.Linq;

namespace ExtraRando.ModInterop.Randomizer;

internal class RandoMenu
{
    #region Members

    private static RandoMenu _instance;
    private MenuPage _connectionPage;
    private MenuElementFactory<RandoSettings> _elementFactory;
    private GridItemPanel _hintPanel;

    private MenuPage _victoryPage;
    private Dictionary<string, IValueElement> _subPageLookup = [];
    private const string ToggleVictoryCondition = "Toggle Victory Conditions";
    private const string ConditionHandling = "Condition Handling";
    private const string WarpToCredits = "Warp To Credits";

    #endregion

    #region Properties

    public static RandoMenu Instance => _instance ??= new();

    #endregion

    #region Event handler

    private void ConstructMenu(MenuPage previousPage)
    {
        VictoryModule.LoadConditions();
        _subPageLookup.Clear();
        // Generate pages and setting elements
        _connectionPage = new("Extra Rando", previousPage);
        _elementFactory = new(_connectionPage, ExtraRando.Instance.Settings);
        SmallButton victoryButton = new(_connectionPage, "Victory Conditions");
        victoryButton.Text.color = !ExtraRando.Instance.Settings.UseVictoryConditions ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;

        _hintPanel = new(_connectionPage, new(0, -275), 4, 0, 300, false, [ 
            _elementFactory.ElementLookup["JunkItemHints"],
            _elementFactory.ElementLookup["PotentialItemHints"],
            _elementFactory.ElementLookup["UsefulItemHints"],
            _elementFactory.ElementLookup["RandomLocationHints"]]);

        VerticalItemPanel leftPanel = new(_connectionPage, new(0, 0), 80f, false, [
            _elementFactory.ElementLookup["RandomizeHotSprings"],
            _elementFactory.ElementLookup["RandomizeColoAccess"],
            _elementFactory.ElementLookup["RandomizePantheonAccess"],
            _elementFactory.ElementLookup["RandomizeButt"],
            _elementFactory.ElementLookup["RandomizeAwfulLocations"],
            _elementFactory.ElementLookup["EnforceJunkLocations"],
            victoryButton
        ]);
        VerticalItemPanel rightPanel = new(_connectionPage, new(0, 0), 80f, false, [
            _elementFactory.ElementLookup["SplitFireball"],
            _elementFactory.ElementLookup["SplitShadeCloak"],
            _elementFactory.ElementLookup["UseKeyring"],
            _elementFactory.ElementLookup["ScarceItemPool"],
            _elementFactory.ElementLookup["BlockEarlyGameStags"],
            _elementFactory.ElementLookup["AddFixedHints"],
            _elementFactory.ElementLookup["AddHintMarkers"]
        ]);
        
        GridItemPanel optionPanels = new(_connectionPage, new(0f, 150), 2, 100f, 600f, false, [leftPanel, rightPanel]);
        new VerticalItemPanel(_connectionPage, new(0, 450f), 150f, true, [_elementFactory.ElementLookup["Enabled"], optionPanels]);
        _elementFactory.ElementLookup["NoLogic"].MoveTo(new(0, -350f));

        _elementFactory.ElementLookup["AddHintMarkers"].SelfChanged += ToggleHintVisibility;
        if (!ExtraRando.Instance.Settings.AddHintMarkers)
            _hintPanel.Hide();
        else
            _hintPanel.Show();

        // Build page.
        _victoryPage = new("Victory Settings", _connectionPage);
        
        victoryButton.AddHideAndShowEvent(_connectionPage, _victoryPage);
        _victoryPage.BeforeGoBack += () => victoryButton.Text.color = !ExtraRando.Instance.Settings.UseVictoryConditions 
            ? Colors.DEFAULT_COLOR 
            : Colors.TRUE_COLOR;

        ToggleButton enableVictoryCondition = new(_victoryPage, "Enabled");
        enableVictoryCondition.Bind(ExtraRando.Instance.Settings, ReflectionHelper.GetPropertyInfo(typeof(RandoSettings), "UseVictoryConditions"));
        _subPageLookup.Add(ToggleVictoryCondition, enableVictoryCondition);

        MenuEnum<VictoryConditionHandling> conditionHandler = new(_victoryPage, "Required conditions");
        conditionHandler.Bind(ExtraRando.Instance.Settings, ReflectionHelper.GetPropertyInfo(typeof(RandoSettings), "ConditionHandling"));
        _subPageLookup.Add(ConditionHandling, conditionHandler);

        ToggleButton victoryMode = new(_victoryPage, "Warp to End");
        victoryMode.Bind(ExtraRando.Instance.Settings, ReflectionHelper.GetPropertyInfo(typeof(RandoSettings), "WarpToCredits"));
        _subPageLookup.Add(WarpToCredits, victoryMode);

        SmallButton resetButton = new(_victoryPage, "Reset");
        
        new VerticalItemPanel(_victoryPage, new(0, 450f), 100f, true, 
        [
            enableVictoryCondition,
            conditionHandler,
            victoryMode,
            resetButton
        ]);

        List<IValueElement> elements = [];
        foreach (IVictoryCondition condition in VictoryModule.AvailableConditions.OrderBy(x => x.GetMenuName()))
        {
            EntryField<int> conditionField = new(_victoryPage, condition.GetMenuName());
            string conditionName = condition.GetType().Name;
            if (!ExtraRando.Instance.Settings.VictoryConditions.ContainsKey(conditionName))
                ExtraRando.Instance.Settings.VictoryConditions.Add(conditionName, 0);

            conditionField.SelfChanged += x => 
            {
                int clampedAmount = condition.ClampAvailableRange((int)x.Value);
                if (clampedAmount != (int)x.Value)
                    conditionField.SetValue(clampedAmount);
                else
                    ExtraRando.Instance.Settings.VictoryConditions[conditionName] = clampedAmount;
            };
            conditionField.SetValue(ExtraRando.Instance.Settings.VictoryConditions[conditionName]);
            elements.Add(conditionField);
            _subPageLookup.Add(conditionName, conditionField);
        }
        new GridItemPanel(_victoryPage, new(0, 0f), 6, 120, 300, true, [..elements]);
        resetButton.OnClick += () => 
        {
            foreach (IValueElement conditionField in elements)
                conditionField.SetValue(0);
        };
    }

    private void ToggleHintVisibility(IValueElement obj)
    {
        if ((bool)obj.Value)
            _hintPanel.Show();
        else
            _hintPanel.Hide();
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
        { 
            _elementFactory.SetMenuValues(randoSettings);
            _subPageLookup[ToggleVictoryCondition].SetValue(randoSettings.UseVictoryConditions);
            _subPageLookup[ConditionHandling].SetValue(randoSettings.ConditionHandling);
            _subPageLookup[WarpToCredits].SetValue(randoSettings.WarpToCredits);

            randoSettings.VictoryConditions ??= [];
            foreach (string key in randoSettings.VictoryConditions.Keys)
                if (_subPageLookup.ContainsKey(key))
                    _subPageLookup[key].SetValue(randoSettings.VictoryConditions[key]);
        }
    }

    #endregion
}
