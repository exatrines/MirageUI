using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// Scrollable list with accent-highlighted multi-select rows. Click toggles selection.
    /// </summary>
    public static void MultiSelectHighlightList(
        string id,
        IReadOnlyList<MirageMultiSelectItem> items,
        HashSet<string> selectedKeys,
        float height = 0f,
        string? emptyText = null,
        Action<string>? onSelectionToggled = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selectedKeys);

        var listHeight = height > 0f
            ? height
            : Math.Max(160f, ImGui.GetContentRegionAvail().Y - 8f);

        var accent = GetColor(Color.Accent);
        using var header = ImRaii.PushColor(ImGuiCol.Header, WithAlpha(accent, 0.32f));
        using var headerHovered = ImRaii.PushColor(ImGuiCol.HeaderHovered, WithAlpha(accent, 0.48f));
        using var headerActive = ImRaii.PushColor(ImGuiCol.HeaderActive, WithAlpha(accent, 0.60f));

        using var child = ImRaii.Child(
            id,
            new Vector2(0f, listHeight),
            true,
            ImGuiWindowFlags.HorizontalScrollbar);
        if (!child)
            return;

        if (items.Count == 0)
        {
            if (!string.IsNullOrEmpty(emptyText))
                Text(emptyText, Color.Secondary);
            return;
        }

        foreach (var item in items)
        {
            var selected = selectedKeys.Contains(item.SelectionKey);
            var label = $"{item.Label}##{item.Id}";
            if (!ImGui.Selectable(label, selected))
                continue;

            if (!selectedKeys.Add(item.SelectionKey))
                selectedKeys.Remove(item.SelectionKey);

            onSelectionToggled?.Invoke(item.SelectionKey);
        }
    }
}
