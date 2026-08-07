using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// Default row: Filter + Sort + Search, then optional trailing action buttons.
    /// Filter kinds and sort targets come from <see cref="MirageIconToolbarState"/>.
    /// </summary>
    public static void IconToolbar(MirageIconToolbarState state, string id = "iconToolbar")
    {
        ArgumentNullException.ThrowIfNull(state);

        using var _ = ImRaii.PushId(id);

        var controlH = ResolveControlHeight();
        var btnSize = new Vector2(controlH);
        var gap = ImGui.GetStyle().ItemSpacing.X;
        var actionCount = state.Actions?.Count ?? 0;
        var leadingCount =
            (state.ShowFilter ? 1 : 0)
            + (state.ShowSort ? 1 : 0);
        var avail = ImGui.GetContentRegionAvail().X;
        var reserved = controlH * (leadingCount + actionCount) + gap * (leadingCount + actionCount);
        var searchW = Math.Max(80f, avail - reserved);

        var first = true;
        void SameLine()
        {
            if (!first)
                ImGui.SameLine(0f, gap);
            first = false;
        }

        if (state.ShowFilter)
        {
            SameLine();
            var filter = state.Filter;
            IconChecklistButton(
                filter.Icon,
                filter.Options,
                filter.SelectedIds,
                filter.Mode,
                id: filter.Id,
                tooltip: filter.Tooltip,
                size: btnSize,
                border: true,
                accentWhenActive: true);
        }

        if (state.ShowSort)
        {
            SameLine();
            DrawToolbarSort(state.Sort, btnSize);
        }

        if (state.ShowSearch)
        {
            SameLine();
            var search = state.Search;
            SearchFilter(state.SearchId, ref search, state.SearchHint, width: searchW);
            state.Search = search;
        }

        if (state.Actions is null)
            return;

        foreach (var action in state.Actions)
        {
            SameLine();
            Vector4? color = action.IsActive?.Invoke() == true ? GetColor(Color.Accent) : null;
            if (IconButton(
                    action.Icon,
                    id: action.Id,
                    size: btnSize,
                    color: color,
                    tooltip: action.Tooltip,
                    border: action.Border))
            {
                action.OnClick?.Invoke();
            }
        }
    }

    private static void DrawToolbarSort(MirageToolbarSortConfig sort, Vector2 btnSize)
    {
        EnsureSortTarget(sort);

        // Multiple targets: open checklist popup (unique target + ascending toggle).
        if (sort.Targets.Count > 1)
        {
            var popupId = $"##{sort.Id}TargetPopup";
            var icon = sort.Ascending ? sort.AscIcon : sort.DescIcon;
            var tooltip = sort.Ascending ? sort.AscTooltip : sort.DescTooltip;
            if (IconButton(icon, id: sort.Id, size: btnSize, tooltip: tooltip, border: true))
                ImGui.OpenPopup(popupId);

            if (!BeginAccentPopup(popupId))
                return;

            var targetIds = new HashSet<string>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(sort.SelectedTargetId))
                targetIds.Add(sort.SelectedTargetId);

            if (DrawChecklistPopupContents(sort.Targets, targetIds, MirageChecklistMode.Unique)
                && targetIds.Count > 0)
            {
                sort.SelectedTargetId = targetIds.First();
            }

            ImGui.Separator();
            var ascending = sort.Ascending;
            if (Checkbox(sort.AscTooltip ?? "Ascending", ref ascending))
                sort.Ascending = ascending;

            EndAccentPopup();
            return;
        }

        // Single / no target: click toggles direction (two-icon swap).
        var ascendingToggle = sort.Ascending;
        if (IconToggleButton(
                sort.DescIcon,
                sort.AscIcon,
                ref ascendingToggle,
                id: sort.Id,
                tooltipOff: sort.DescTooltip,
                tooltipOn: sort.AscTooltip,
                size: btnSize,
                border: true))
        {
            sort.Ascending = ascendingToggle;
        }
    }

    private static void EnsureSortTarget(MirageToolbarSortConfig sort)
    {
        if (sort.Targets.Count == 0)
            return;

        if (sort.Targets.Any(t => t.Id == sort.SelectedTargetId))
            return;

        sort.SelectedTargetId = sort.Targets[0].Id;
    }
}
