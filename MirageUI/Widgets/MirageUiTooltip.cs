using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    public static void Tooltip(
        string text,
        MirageTooltipPosition position = MirageTooltipPosition.Cursor,
        bool allowWhenDisabled = false)
    {
        var hovered = allowWhenDisabled
            ? ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)
            : ImGui.IsItemHovered();
        if (!hovered || string.IsNullOrEmpty(text))
            return;

        MirageTooltip.Show(text, position);
    }
}
