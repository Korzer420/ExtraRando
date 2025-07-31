using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using RandomizerCore.Logic;

namespace ExtraRando.Data;

/// <summary>
/// Represents a condition used in the <see cref="VictoryModule"/>.
/// <para/>To initiate a victory check, call <see cref="VictoryModule.CheckForFinish"/>.
/// </summary>
public interface IVictoryCondition
{
    /// <summary>
    /// Gets or sets the current amount.
    /// <para/>If this is bigger than <see cref="RequiredAmount"/>, the condition is considered met.
    /// </summary>
    public int CurrentAmount { get; set; }

    /// <summary>
    /// Gets or sets the required amount.
    /// </summary>
    public int RequiredAmount { get; set; }

    /// <summary>
    /// Gets the name that the mod should display in the victory settings page.
    /// </summary>
    public string GetMenuName();

    /// <summary>
    /// Gets the logic that is used for black egg temple access (e.g. for world sense).
    /// <para/>This method is called when the victory condition should be used and should serve as a setup opportunity for the logic in question.
    /// <para/>Keep in mind that you need to handle the required amount yourself!
    /// </summary>
    public string PrepareLogic(LogicManagerBuilder logicBuilder);

    /// <summary>
    /// Gets the text displayed as a hint on the hint tablet.
    /// <para/>If this condition is relying on ItemChanger items, you might wanna fetch the area and provide that as a hint.
    /// <para/>For example: "3 in Crystal Peak, 2 in Crossroads".
    /// </summary>
    public string GetHintText();

    /// <summary>
    /// Clamps the set menu number in between the allowed value.
    /// Return a valid value.
    /// </summary>
    /// <param name="setAmount">The input of the user which should be verified.</param>
    public int ClampAvailableRange(int setAmount);

    /// <summary>
    /// Starts listening to track possible changes.
    /// <para/>This is called when the <see cref="VictoryModule"/> is initialized.
    /// </summary>
    public void StartListening();

    /// <summary>
    /// Gives an opportunity to remove set hooks.
    /// <para/>This is called when the <see cref="VictoryModule"/> is unloaded.
    /// </summary>
    public void StopListening();
}
