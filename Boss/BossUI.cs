using BTD_Mod_Helper.Api.Components;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BossIntegration.Boss;

/// <summary>
/// All the UI components for a boss
/// </summary>
public struct BossUI
{
    /// <summary>
    /// Round on which this object was created
    /// </summary>
    public int? round;

    /// <summary>
    /// Parent of the UI
    /// </summary>
    public ModHelperPanel Panel;

    /// <summary>
    /// Image used to display the health slider
    /// </summary>
    public Image HpBar;

    /// <summary>
    /// Text used to display the health in text
    /// </summary>
    public ModHelperText HpText;

    /// <summary>
    /// Where the first object is the last skull to activate
    /// </summary>
    public List<ModHelperImage> Skulls;
}
