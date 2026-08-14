using MirageUI.Theme;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    private const float DropdownPopupRounding = 4f;
    private const float DropdownPopupPaddingX = 6f;
    private const float DropdownPopupPaddingY = 6f;

    /// <summary>
    /// Default width: fill the field column (table layout) or half content when unlabeled.
    /// </summary>
    public const float DropdownWidthHalfMain = 0f;

    private static readonly Stack<(ImRaii.ColorDisposable Colors, ImRaii.StyleDisposable Styles, bool UsedTable)> DropdownScopes = new();

    /// <summary>
    /// Accent-colored dropdown. Returns true when the selection changes.
    /// Layout: [label | combo] via Table API.
    /// </summary>
    public static bool Dropdown(
        string label,
        ref string selected,
        IReadOnlyList<string> items,
        string? placeholder = "(not set)",
        string id = "",
        bool allowClear = true,
        string? emptyMessage = null,
        float width = InputWidthFill,
        bool? liveMatch = null,
        string? matchTooltip = null,
        float labelWidth = FieldLabelColumnWidth)
    {
        var preview = string.IsNullOrWhiteSpace(selected)
            ? (placeholder ?? string.Empty)
            : selected;

        if (!BeginDropdown(
                label,
                preview,
                id,
                width,
                hasValue: !string.IsNullOrWhiteSpace(selected),
                liveMatch: liveMatch,
                matchTooltip: matchTooltip,
                labelWidth: labelWidth))
            return false;

        var changed = DrawDropdownItems(ref selected, items, placeholder, allowClear, emptyMessage, filter: null);
        EndDropdown();
        return changed;
    }

    /// <summary>
    /// Accent-colored searchable dropdown. Filters items by <paramref name="searchFilter"/>.
    /// Layout: [label | combo] via Table API.
    /// When <paramref name="headerButtonLabel"/> is set, the popup header is
    /// [search 50% | button 50%].
    /// </summary>
    public static bool SearchableDropdown(
        string label,
        ref string selected,
        IReadOnlyList<string> items,
        ref string searchFilter,
        string? placeholder = "(not set)",
        string id = "",
        bool allowClear = true,
        string? emptyMessage = null,
        string searchHint = "Search…",
        float width = InputWidthFill,
        string? headerButtonLabel = null,
        Action? onHeaderButton = null,
        bool? liveMatch = null,
        string? matchTooltip = null,
        float labelWidth = FieldLabelColumnWidth)
    {
        var preview = string.IsNullOrWhiteSpace(selected)
            ? (placeholder ?? string.Empty)
            : selected;

        if (!BeginDropdown(
                label,
                preview,
                id,
                width,
                hasValue: !string.IsNullOrWhiteSpace(selected),
                comboFlags: ImGuiComboFlags.HeightLarge,
                liveMatch: liveMatch,
                matchTooltip: matchTooltip,
                labelWidth: labelWidth))
            return false;

        if (ImGui.IsWindowAppearing())
            searchFilter = string.Empty;

        ImGui.PushID(string.IsNullOrEmpty(id) ? label : id);

        var hasHeaderButton = !string.IsNullOrWhiteSpace(headerButtonLabel);
        if (hasHeaderButton)
        {
            var avail = ImGui.GetContentRegionAvail().X;
            var gap = MirageLayout.Style.ItemSpacing.X;
            var half = Math.Max(1f, (avail - gap) * 0.5f);
            SearchFilter("##Search"u8, ref searchFilter, searchHint, width: half);
            ImGui.SameLine(0f, gap);
            if (PrimaryButton(headerButtonLabel!, width: half, id: "headerAction"))
            {
                onHeaderButton?.Invoke();
                ImGui.CloseCurrentPopup();
            }
        }
        else
        {
            SearchFilter("##Search"u8, ref searchFilter, searchHint);
        }

        ImGui.PopID();

        // Keep the search bar fixed; only the item list scrolls.
        ImGui.Separator();
        var listHeight = MathF.Max(
            ImGui.GetTextLineHeightWithSpacing() * 8f,
            ImGui.GetContentRegionAvail().Y);
        var changed = false;
        if (ImGui.BeginChild(
                "##DropdownItems"u8,
                new Vector2(0f, listHeight),
                false,
                ImGuiWindowFlags.HorizontalScrollbar))
        {
            changed = DrawDropdownItems(
                ref selected,
                items,
                placeholder,
                allowClear,
                emptyMessage,
                filter: searchFilter);
        }

        ImGui.EndChild();
        EndDropdown();
        return changed;
    }

    private static bool DrawDropdownItems(
        ref string selected,
        IReadOnlyList<string> items,
        string? placeholder,
        bool allowClear,
        string? emptyMessage,
        string? filter)
    {
        var changed = false;

        if (allowClear && placeholder != null)
        {
            var clearVisible = string.IsNullOrWhiteSpace(filter)
                               || MatchesFilter(placeholder, placeholder, filter);
            if (clearVisible && DropdownItem(placeholder, string.IsNullOrWhiteSpace(selected)))
            {
                if (!string.IsNullOrWhiteSpace(selected))
                {
                    selected = string.Empty;
                    changed = true;
                }
            }
        }

        var visibleCount = 0;
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(filter) && !MatchesFilter(item, item, filter))
                continue;

            visibleCount++;
            var isSelected = string.Equals(selected, item, StringComparison.Ordinal);
            if (!DropdownItem(item, isSelected))
                continue;

            if (isSelected)
                continue;

            selected = item;
            changed = true;
        }

        if (visibleCount == 0 && items.Count == 0 && !string.IsNullOrEmpty(emptyMessage))
            ImGui.TextDisabled(emptyMessage);
        else if (visibleCount == 0 && !string.IsNullOrWhiteSpace(filter))
            ImGui.TextDisabled("No matches");

        return changed;
    }

    /// <summary>
    /// Starts an accent-colored dropdown inside a [label | field] table row.
    /// When true, draw items then call <see cref="EndDropdown"/>.
    /// </summary>
    public static bool BeginDropdown(
        string label,
        string preview,
        string id = "",
        float width = InputWidthFill,
        bool hasValue = true,
        ImGuiComboFlags comboFlags = ImGuiComboFlags.None,
        bool? liveMatch = null,
        string? matchTooltip = null,
        float labelWidth = FieldLabelColumnWidth)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, liveMatch, matchTooltip, labelWidth, width))
            return false;

        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var accent = settings.GetColor(Color.Accent);
        var (frame, hovered, active, border, header, headerHovered, headerActive, popupBg) =
            GetDropdownColors(settings, accent);

        var framePadding = ResolveInputFramePadding();
        var scale = MirageLayout.Style.Scale;
        var popupPadding = new Vector2(
            DropdownPopupPaddingX * scale,
            DropdownPopupPaddingY * scale);

        var colors = new ImRaii.ColorDisposable()
            .Push(ImGuiCol.FrameBg, frame)
            .Push(ImGuiCol.FrameBgHovered, hovered)
            .Push(ImGuiCol.FrameBgActive, active)
            .Push(ImGuiCol.Border, border)
            .Push(ImGuiCol.Header, header)
            .Push(ImGuiCol.HeaderHovered, headerHovered)
            .Push(ImGuiCol.HeaderActive, headerActive)
            .Push(ImGuiCol.PopupBg, popupBg)
            .Push(ImGuiCol.Button, frame)
            .Push(ImGuiCol.ButtonHovered, hovered)
            .Push(ImGuiCol.ButtonActive, active)
            .Push(ImGuiCol.CheckMark, accent);

        var styles = new ImRaii.StyleDisposable()
            .Push(ImGuiStyleVar.FrameRounding, InputStyle.FrameRounding)
            .Push(ImGuiStyleVar.PopupRounding, DropdownPopupRounding)
            .Push(ImGuiStyleVar.FrameBorderSize, 1f)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f)
            .Push(ImGuiStyleVar.FramePadding, framePadding)
            .Push(ImGuiStyleVar.WindowPadding, popupPadding);

        DropdownScopes.Push((colors, styles, usedTable));

        var comboId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
        var previewColor = hasValue
            ? settings.GetColor(Color.Default)
            : settings.GetColor(Color.Secondary);
        var controlWidth = ResolveFieldControlWidth(width, usedTable);

        ImGui.SetNextItemWidth(controlWidth);
        ImGui.PushStyleColor(ImGuiCol.Text, previewColor);
        var open = ImGui.BeginCombo(comboId, preview, comboFlags);
        ImGui.PopStyleColor();

        if (!open)
        {
            PopDropdownScope();
            return false;
        }

        // Open: popup content is drawn by the caller before EndDropdown.
        return true;
    }

    /// <summary>Call when <see cref="BeginDropdown"/> returns true.</summary>
    public static void EndDropdown()
    {
        if (DropdownScopes.Count == 0)
        {
            ImGui.EndCombo();
            return;
        }

        ImGui.EndCombo();
        PopDropdownScope();
    }

    /// <summary>Dropdown item. Focuses the selected row when open.</summary>
    public static bool DropdownItem(string label, bool selected)
    {
        var clicked = ImGui.Selectable(label, selected);
        if (selected)
            ImGui.SetItemDefaultFocus();
        return clicked;
    }

    private static void PopDropdownScope()
    {
        if (DropdownScopes.Count == 0)
            return;

        var (colors, styles, usedTable) = DropdownScopes.Pop();
        styles.Dispose();
        colors.Dispose();
        EndFieldRow(usedTable);
    }

    private static (
        Vector4 Frame,
        Vector4 Hovered,
        Vector4 Active,
        Vector4 Border,
        Vector4 Header,
        Vector4 HeaderHovered,
        Vector4 HeaderActive,
        Vector4 PopupBg)
        GetDropdownColors(MirageColorSettings settings, Vector4 accent)
    {
        var frame = WithAlpha(accent, 0.14f);
        var hovered = WithAlpha(accent, 0.26f);
        var active = WithAlpha(accent, 0.38f);
        var border = WithAlpha(accent, 0.75f);
        var header = WithAlpha(accent, 0.32f);
        var headerHovered = WithAlpha(accent, 0.48f);
        var headerActive = WithAlpha(accent, 0.60f);
        var popupBg = MixColors(settings.WindowBg, accent, 0.08f);
        return (frame, hovered, active, border, header, headerHovered, headerActive, popupBg);
    }
}
