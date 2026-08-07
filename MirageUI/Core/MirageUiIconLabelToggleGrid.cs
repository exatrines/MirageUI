using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    private const float IconLabelBorderRounding = 4f;
    private const float IconLabelBorderThickness = 1.5f;
    private const float IconLabelRowSelectionRounding = 4f;
    private const float IconLabelRowSelectionThickness = 1.5f;
    private static readonly Vector4 IconLabelFrameBorder = new(1f, 1f, 1f, 0.85f);

    /// <summary>
    /// Icon + label toggle tiles in a multi-column table.
    /// Multi-select toggles membership; single-select keeps only the clicked id.
    /// </summary>
    public static void IconLabelToggleGrid(
        string tableId,
        IReadOnlyList<(uint Id, uint IconId, string Label)> items,
        HashSet<uint> selected,
        int columns = 2,
        float? iconSize = null,
        bool singleSelect = false) =>
        IconLabelToggleGrid(tableId, items, selected, out _, columns, iconSize, singleSelect);

    /// <summary>
    /// Draws the grid. When <paramref name="singleSelect"/> is true, clicking selects only that id
    /// and returns it via <paramref name="clickedId"/>.
    /// </summary>
    public static void IconLabelToggleGrid(
        string tableId,
        IReadOnlyList<(uint Id, uint IconId, string Label)> items,
        HashSet<uint> selected,
        out uint? clickedId,
        int columns = 2,
        float? iconSize = null,
        bool singleSelect = false)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selected);

        clickedId = null;
        if (items.Count == 0 || columns < 1)
            return;

        var scale = MirageLayout.Style.Scale;
        var buttonSize = iconSize ?? MirageLayout.Style.FrameHeight;
        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        var columnGap = 8f * scale;
        var rowPad = 2f * scale;

        if (!ImGui.BeginTable(
                tableId,
                columns,
                ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.NoSavedSettings))
            return;

        for (var c = 0; c < columns; c++)
            ImGui.TableSetupColumn($"c{c}", ImGuiTableColumnFlags.WidthStretch);

        foreach (var item in items)
        {
            ImGui.TableNextColumn();
            using var pushId = ImRaii.PushId((int)item.Id);

            var isSelected = selected.Contains(item.Id);
            var rowWidth = MathF.Max(0f, MirageLayout.Style.ContentRegionAvail.X - columnGap * 0.5f);
            var textLineHeight = ImGui.GetTextLineHeight();
            var rowHeight = MathF.Max(buttonSize, textLineHeight) + rowPad * 2f;

            if (!DrawIconLabelToggleRow(
                    item.IconId,
                    item.Label,
                    rowWidth,
                    rowHeight,
                    buttonSize,
                    gap,
                    rowPad,
                    isSelected))
                continue;

            clickedId = item.Id;
            if (singleSelect)
            {
                selected.Clear();
                selected.Add(item.Id);
            }
            else if (!selected.Add(item.Id))
            {
                selected.Remove(item.Id);
            }
        }

        ImGui.EndTable();
    }

    private static bool DrawIconLabelToggleRow(
        uint iconId,
        string label,
        float rowWidth,
        float rowHeight,
        float buttonSize,
        float gap,
        float rowPad,
        bool selected)
    {
        var clicked = ImGui.InvisibleButton("##row", new Vector2(rowWidth, rowHeight));
        var rowMin = ImGui.GetItemRectMin();
        var rowMax = ImGui.GetItemRectMax();
        var dl = ImGui.GetWindowDrawList();
        var scale = MirageLayout.Style.Scale;

        var contentMin = rowMin + new Vector2(rowPad, rowPad);
        var iconMin = contentMin;
        var iconMax = iconMin + new Vector2(buttonSize, buttonSize);
        var titleColor = GetColor(Color.Default);

        if (TryGetGameIcon(iconId, out var texture))
            dl.AddImage(texture.Handle, iconMin, iconMax);

        var iconFrameInset = 1.5f * scale;
        dl.AddRect(
            iconMin + new Vector2(iconFrameInset, iconFrameInset),
            iconMax - new Vector2(iconFrameInset, iconFrameInset),
            ImGui.ColorConvertFloat4ToU32(IconLabelFrameBorder),
            IconLabelBorderRounding * scale,
            ImDrawFlags.RoundCornersAll,
            IconLabelBorderThickness * scale);

        var contentX = contentMin.X + buttonSize + gap;
        var titleSize = ImGui.CalcTextSize(label);
        var titleY = contentMin.Y + MathF.Max(0f, (buttonSize - titleSize.Y) * 0.5f);
        dl.AddText(new Vector2(contentX, titleY), ImGui.ColorConvertFloat4ToU32(titleColor), label);

        if (selected)
        {
            dl.AddRect(
                rowMin,
                rowMax,
                ImGui.ColorConvertFloat4ToU32(GetColor(Color.Accent)),
                IconLabelRowSelectionRounding * scale,
                ImDrawFlags.RoundCornersAll,
                IconLabelRowSelectionThickness * scale);
        }

        return clicked;
    }
}
