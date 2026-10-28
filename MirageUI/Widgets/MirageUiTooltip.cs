using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    public static void Tooltip(
        string text,
        MirageTooltipPosition position = MirageTooltipPosition.Cursor)
    {
        if (!ImGui.IsItemHovered() || string.IsNullOrEmpty(text))
            return;

        MirageTooltip.Show(text, position);
    }
}
