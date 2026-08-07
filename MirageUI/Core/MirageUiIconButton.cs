using Dalamud.Interface;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>FontAwesome アイコンボタン。アイコンは中央揃え。</summary>
    public static bool IconButton(
        FontAwesomeIcon icon,
        string id = "",
        Vector2 size = default,
        Vector4? color = null,
        Vector4? hoverColor = null,
        FontAwesomeIcon? hoverIcon = null,
        Vector4? backgroundColor = null,
        Vector4? hoveredBackgroundColor = null,
        Vector4? activeBackgroundColor = null,
        string? tooltip = null,
        MirageTooltipPosition tooltipPosition = MirageTooltipPosition.Cursor,
        bool enabled = true,
        bool border = false) =>
        MirageIconButton.Draw(
            icon,
            hoverIcon,
            id,
            size,
            color ?? MirageIconButton.DefaultText,
            hoverColor,
            backgroundColor ?? MirageIconButton.DefaultBackground,
            hoveredBackgroundColor,
            activeBackgroundColor,
            tooltip,
            tooltipPosition,
            enabled,
            border);

    /// <summary>
    /// Icon button that swaps between two icons. Click toggles <paramref name="isOn"/>.
    /// When <paramref name="accentWhenOn"/> is true and on, icon uses accent color.
    /// </summary>
    public static bool IconToggleButton(
        FontAwesomeIcon iconOff,
        FontAwesomeIcon iconOn,
        ref bool isOn,
        string id = "",
        string? tooltipOff = null,
        string? tooltipOn = null,
        Vector2 size = default,
        bool border = true,
        bool accentWhenOn = false,
        bool enabled = true)
    {
        var icon = isOn ? iconOn : iconOff;
        var tooltip = isOn ? tooltipOn : tooltipOff;
        Vector4? color = accentWhenOn && isOn ? GetColor(Color.Accent) : null;
        if (!IconButton(icon, id, size, color, tooltip: tooltip, enabled: enabled, border: border))
            return false;

        isOn = !isOn;
        return true;
    }

    /// <summary>
    /// Icon button that opens an accent checklist popup (unique or multi).
    /// Accent tint when any option is selected and <paramref name="accentWhenActive"/> is true.
    /// Returns true when the selection changed.
    /// </summary>
    public static bool IconChecklistButton(
        FontAwesomeIcon icon,
        IReadOnlyList<MirageChecklistOption> options,
        HashSet<string> selectedIds,
        MirageChecklistMode mode = MirageChecklistMode.Multi,
        string id = "",
        string? tooltip = null,
        Vector2 size = default,
        bool border = true,
        bool accentWhenActive = true,
        bool enabled = true)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(selectedIds);

        var popupId = string.IsNullOrEmpty(id) ? "##iconChecklistPopup" : $"##{id}Popup";
        var active = accentWhenActive && selectedIds.Count > 0;
        Vector4? color = active ? GetColor(Color.Accent) : null;

        if (IconButton(icon, id, size, color, tooltip: tooltip, enabled: enabled, border: border))
            ImGui.OpenPopup(popupId);

        return FilterIconPopup(popupId, options, selectedIds, mode);
    }
}

