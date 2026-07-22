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
        bool enabled = true) =>
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
            enabled);
}
