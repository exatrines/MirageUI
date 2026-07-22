using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// 直前のアイテムに対するツールチップ。
    /// <paramref name="position"/> が <see cref="MirageTooltipPosition.Above"/> /
    /// <see cref="MirageTooltipPosition.Below"/> のときはアイテム矩形を基準にする。
    /// </summary>
    public static void Tooltip(
        string text,
        MirageTooltipPosition position = MirageTooltipPosition.Cursor)
    {
        if (!ImGui.IsItemHovered() || string.IsNullOrEmpty(text))
            return;

        MirageTooltip.Show(text, position);
    }
}
