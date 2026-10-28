namespace MirageUI.Layout;

using Dalamud.Interface;
using MirageUI.Theme;

internal static class TwoColumnLayout
{
    internal static void DrawWindowBlur()
    {
        if (ImGui.GetWindowViewport().ID != ImGui.GetMainViewport().ID)
            return;

        var strength = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).BlurStrength;
        if (strength <= 0f)
            return;

        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();
        ImGuiHelpers.PrependBlurBehind(
            ImGui.GetWindowDrawList(),
            min,
            max,
            blurStrength: strength,
            rounding: ImGui.GetStyle().WindowRounding,
            noiseOpacity: 0f);
    }

    internal static void Draw(
        MirageTwoColumnState state,
        Action drawMainContent)
    {
        MiragePluginPage.BeginHost();
        try
        {
            DrawHost(state, drawMainContent);
        }
        finally
        {
            MiragePluginPage.EndHost();
        }
    }

    private static void DrawHost(
        MirageTwoColumnState state,
        Action drawMainContent)
    {
        var scale = MirageLayout.Style.Scale;
        var startPos = MirageLayout.Cursor.Position;
        DrawWindowBlur();

        if (!state.ShowSidebar)
        {
            DrawContentPanels(state, scale, drawMainContent);
            return;
        }

        var margin = MenuMargin * scale;
        var gap = MenuGap * scale;
        var button = MirageUi.ResolveControlHeight();
        var cardHeight = Math.Max(button, MirageLayout.Style.ContentRegionAvail.Y - margin * 2f);
        var cardWidth = state.SidebarCollapsed ? button : state.SidebarWidth * scale;
        var cardPos = startPos + new Vector2(margin, margin);
        var cardSize = new Vector2(cardWidth, cardHeight);

        MirageLayout.Cursor.Position = cardPos;
        DrawMenuCard(MirageLayout.Cursor.ScreenPosition, cardSize, scale);

        if (state.SidebarCollapsed)
        {
            DrawCollapsedTab(state, cardSize, scale);
            MirageLayout.Cursor.Position = new Vector2(startPos.X + margin + cardWidth + gap, startPos.Y);
            DrawContentPanels(state, scale, drawMainContent);
            return;
        }

        var layout = ComputeSidebarLayout(state, scale, cardHeight);

        using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero))
        using (var sidebarChild = ImRaii.Child(
            "##TwoColumnSidebar"u8,
            new Vector2(cardWidth, cardHeight),
            false,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            if (sidebarChild)
            {
                if (layout.TopSearchHeight > 0f)
                {
                    MirageLayout.Cursor.Position = new Vector2(0f, layout.TopSearchY);
                    DrawSearch(state, cardWidth, scale, layout.TopSearchHeight);
                }

                MirageLayout.Cursor.Position = new Vector2(layout.ListX, layout.ListTop);
                DrawSidebarList(
                    state,
                    cardWidth,
                    scale,
                    layout.ListHeight,
                    layout.HasSearch);
            }
        }

        MirageLayout.Cursor.Position = new Vector2(startPos.X + margin + cardWidth + gap, startPos.Y);
        DrawContentPanels(state, scale, drawMainContent);
    }

    private const float MenuMargin = 12f;
    private const float MenuGap = 12f;
    private const float MenuRounding = 8f;

    private readonly struct SidebarLayout
    {
        public required float TopSearchY { get; init; }
        public required float TopSearchHeight { get; init; }
        public required float ListX { get; init; }
        public required float ListTop { get; init; }
        public required float ListHeight { get; init; }
        public required bool HasSearch { get; init; }
    }

    private static SidebarLayout ComputeSidebarLayout(
        MirageTwoColumnState state,
        float scale,
        float contentHeight)
    {
        var windowPadding = state.SidebarPadding * scale;
        var framePadding = state.SearchFramePadding * scale;
        var hasSearch = state.ShowSearch;
        var topSearchHeight = hasSearch ? GetTopSearchSectionHeight(scale, state) : 0f;

        var listTop = windowPadding.Y;
        if (topSearchHeight > 0f)
            listTop += topSearchHeight - framePadding.Y;

        var listBottom = contentHeight - windowPadding.Y;
        var listHeight = Math.Max(0f, listBottom - listTop);

        return new SidebarLayout
        {
            TopSearchY = topSearchHeight > 0f ? windowPadding.Y : 0f,
            TopSearchHeight = topSearchHeight,
            ListX = windowPadding.X,
            ListTop = listTop,
            ListHeight = listHeight,
            HasSearch = hasSearch,
        };
    }

    private static void DrawMenuCard(Vector2 screenPos, Vector2 size, float scale)
    {
        var rounding = MenuRounding * scale;
        var max = screenPos + size;
        var dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(
            screenPos,
            max,
            ImGui.GetColorU32((MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).ChromeBg),
            rounding);
        dl.AddRect(screenPos, max, ImGui.GetColorU32(ImGuiCol.Separator), rounding);
    }

    private static void DrawCollapsedTab(MirageTwoColumnState state, Vector2 size, float scale)
    {
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        using var child = ImRaii.Child(
            "##SidebarToggle"u8,
            size,
            false,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground);
        if (!child)
            return;

        if (ImGui.InvisibleButton("##SidebarCollapse", size))
            state.SidebarCollapsed = false;

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var hovered = ImGui.IsItemHovered();
        if (hovered)
        {
            var held = ImGui.IsMouseDown(ImGuiMouseButton.Left);
            var fill = ImGui.GetColorU32(held ? ImGuiCol.HeaderActive : ImGuiCol.HeaderHovered);
            ImGui.GetWindowDrawList().AddRectFilled(min, max, fill, MenuRounding * scale);
            MirageUi.Tooltip(MirageUi.I18n.Get("window.expand"));
        }

        var icon = FontAwesomeIcon.EllipsisV.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon);
        var iconPos = min + (size - iconSize) * 0.5f;
        var color = hovered
            ? MirageUi.ToUInt(MirageUi.GetColor(MirageUi.Color.Accent))
            : ImGui.GetColorU32(ImGuiCol.Text);
        ImGui.GetWindowDrawList().AddText(iconPos, color, icon);
        ImGui.PopFont();
    }

    private static bool MenuToggleButton(float button)
    {
        return MirageUi.IconButton(
            FontAwesomeIcon.Times,
            "##SidebarCollapse",
            new Vector2(button, button),
            MirageUi.GetColor(MirageUi.Color.Default),
            tooltip: MirageUi.I18n.Get("window.close"));
    }

    private static float GetSearchInputHeight(float scale, MirageTwoColumnState state)
    {
        var framePadding = state.SearchFramePadding * scale;
        var inputHeight = ImGui.GetFontSize() + framePadding.Y * 2f;
        if (state.SearchTrailingActions.Count == 0)
            return inputHeight;

        return Math.Max(inputHeight, MirageUi.ResolveControlHeight());
    }

    private static float GetTopSearchSectionHeight(float scale, MirageTwoColumnState state) =>
        GetSearchInputHeight(scale, state) + state.SidebarPadding.Y * scale;

    private static readonly ImGuiWindowFlags FixedSectionFlags =
        ImGuiWindowFlags.AlwaysUseWindowPadding
        | ImGuiWindowFlags.NoScrollbar
        | ImGuiWindowFlags.NoScrollWithMouse;

    private static void DrawSearch(
        MirageTwoColumnState state,
        float sidebarWidth,
        float scale,
        float sectionHeight)
    {
        var windowPadding = state.SidebarPadding * scale;
        var framePadding = state.SearchFramePadding * scale;

        using var childStyle = ImRaii.PushStyle(
            ImGuiStyleVar.WindowPadding,
            new Vector2(windowPadding.X, 0f));
        using var child = ImRaii.Child(
            "##TwoColumnSearch"u8,
            new Vector2(sidebarWidth, sectionHeight),
            false,
            FixedSectionFlags);
        if (!child)
            return;

        childStyle.Dispose();

        using var frameStyle = ImRaii
            .PushStyle(ImGuiStyleVar.FramePadding, framePadding)
            .Push(ImGuiStyleVar.FrameRounding, 3);

        var actions = state.SearchTrailingActions;
        var buttonSize = MirageUi.ResolveControlHeight();
        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        var hasActions = actions.Count > 0;
        var actionsWidth = hasActions
            ? (actions.Count * buttonSize) + ((actions.Count - 1) * gap)
            : 0f;
        var divider = 1f;
        var closeWidth = hasActions
            ? gap + divider + gap + buttonSize
            : gap + buttonSize;
        var searchWidth = Math.Max(
            40f,
            MirageLayout.Style.ContentRegionAvail.X - actionsWidth - (hasActions ? gap : 0f) - closeWidth);

        var searchFilter = state.SearchFilter;
        var changed = MirageUi.SearchFilter(
            "##TwoColumnSearchInput"u8,
            ref searchFilter,
            state.SearchHint,
            state.SearchMaxLength,
            width: searchWidth);
        state.SearchFilter = searchFilter;
        state.OnSearchFilterChanged?.Invoke(searchFilter);

        if (hasActions)
        {
            ImGui.SameLine(0f, gap);
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                using var actionId = ImRaii.PushId(action.Id);
                if (i > 0)
                    ImGui.SameLine(0f, gap);

                if (MirageUi.IconButton(
                        action.Icon,
                        "##searchTrail",
                        new Vector2(buttonSize, buttonSize),
                        tooltip: action.Tooltip))
                {
                    if (action.ContextMenuItems.Count > 0)
                        ImGui.OpenPopup("##TrailContext");
                    else
                        action.OnClick?.Invoke();
                }

                DrawSearchActionMenu(action);
            }
        }

        var inputHeight = GetSearchInputHeight(scale, state);
        ImGui.SameLine(0f, gap);
        if (hasActions)
        {
            var lineTop = MirageLayout.Cursor.ScreenPosition;
            var inset = 4f * scale;
            ImGui.GetWindowDrawList().AddLine(
                lineTop + new Vector2(0f, inset),
                lineTop + new Vector2(0f, Math.Max(inset, inputHeight - inset)),
                ImGui.GetColorU32(ImGuiCol.Separator));
            ImGui.Dummy(new Vector2(divider, inputHeight));
            ImGui.SameLine(0f, gap);
        }

        if (inputHeight > buttonSize)
            MirageLayout.Cursor.Y += (inputHeight - buttonSize) * 0.5f;
        if (MenuToggleButton(buttonSize))
            state.SidebarCollapsed = true;

        if (!changed || !state.AutoSelectFirstOnSearch)
            return;

        // Keep selection when clearing; jump to first match while filtering.
        if (string.IsNullOrWhiteSpace(state.SearchFilter))
            return;

        var firstEntry = GetFirstVisibleEntry(state);
        if (firstEntry == null)
            return;

        state.SelectedId = firstEntry.Id;
        MiragePluginPage.Hide();
        state.OnSelectionChanged?.Invoke(firstEntry.Id);
    }

    private static void DrawSearchActionMenu(MirageTwoColumnTrailingAction action)
    {
        if (action.ContextMenuItems.Count == 0)
            return;

        var labels = new string[action.ContextMenuItems.Count];
        for (var i = 0; i < action.ContextMenuItems.Count; i++)
            labels[i] = action.ContextMenuItems[i].Label;

        var style = MirageContextMenuStyle.CreateDefault();
        if (!MirageUi.ContextMenu.Begin("##TrailContext", labels, style))
            return;

        foreach (var item in action.ContextMenuItems)
        {
            var icon = item.Icon ?? FontAwesomeIcon.Circle;
            if (!MirageUi.ContextMenu.DrawItem(item.Label, icon, item.Id, style))
                continue;

            item.OnClick?.Invoke();
            ImGui.CloseCurrentPopup();
        }

        MirageUi.ContextMenu.End();
    }

    private static void DrawSidebarList(
        MirageTwoColumnState state,
        float sidebarWidth,
        float scale,
        float listHeight,
        bool hasSearch)
    {
        var windowPadding = state.SidebarPadding * scale;

        using var paddingStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        using var child = ImRaii.Child(
            "##TwoColumnSidebarList"u8,
            new Vector2(sidebarWidth - windowPadding.X * 2f, listHeight),
            false,
            ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!child)
            return;

        using var spacingStyle = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        if (UsesGroupedSidebar(state))
        {
            var reorderEnabled = state.EnableEntryReorder
                && (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter));
            if (!reorderEnabled && !string.IsNullOrEmpty(state.EntryReorderDragId))
            {
                state.EntryReorderDragId = null;
                state.OnEntryReorderDragIdChanged?.Invoke(null);
            }

            foreach (var node in GetVisibleSidebarNodes(state, hasSearch))
            {
                switch (node)
                {
                    case MirageTwoColumnPageNode page:
                        DrawListItem(state, page.Entry);
                        break;
                    case MirageTwoColumnFolderNode folder:
                        DrawFolderNode(state, folder, scale, reorderEnabled);
                        break;
                }
            }

            return;
        }

        var flatReorderEnabled = state.EnableEntryReorder
            && (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter));
        if (!flatReorderEnabled && !string.IsNullOrEmpty(state.EntryReorderDragId))
        {
            state.EntryReorderDragId = null;
            state.OnEntryReorderDragIdChanged?.Invoke(null);
        }

        var rowBounds = new List<(string Id, float Top, float Bottom)>();
        foreach (var entry in GetVisibleEntries(state, hasSearch))
        {
            var top = MirageLayout.Cursor.ScreenPosition.Y;
            DrawListItem(state, entry, reorderEnabled: flatReorderEnabled);
            rowBounds.Add((entry.Id, top, top + MirageLayout.Style.FrameHeight));
        }

        if (flatReorderEnabled)
            UpdateEntryReorder(state, rowBounds, scale);
    }

    private static bool UsesGroupedSidebar(MirageTwoColumnState state) =>
        state.SidebarNodes.Count > 0;

    private static void DrawFolderNode(
        MirageTwoColumnState state,
        MirageTwoColumnFolderNode folder,
        float scale,
        bool reorderEnabled)
    {
        using var folderId = ImRaii.PushId(folder.Id);

        ImGui.AlignTextToFramePadding();
        var rowStart = MirageLayout.Cursor.Position;
        var rowHeight = MirageUi.ResolveControlHeight();
        var forceExpanded = HasActiveSearch(state);
        var expanded = folder.AlwaysExpanded
            || forceExpanded
            || !state.CollapsedFolderIds.Contains(folder.Id);

        if (ListSelectable.DrawFolderHeader(
                folder.Label,
                expanded,
                rowHeight,
                canCollapse: !folder.AlwaysExpanded)
            && !folder.AlwaysExpanded
            && !forceExpanded)
        {
            if (expanded)
                state.CollapsedFolderIds.Add(folder.Id);
            else
                state.CollapsedFolderIds.Remove(folder.Id);
        }

        MirageLayout.Cursor.Y = rowStart.Y + rowHeight + state.ItemSpacing * scale;

        if (!expanded)
            return;

        var indent = state.FolderPageIndent * scale;
        var rowBounds = new List<(string Id, float Top, float Bottom)>(folder.Entries.Count);
        foreach (var entry in folder.Entries)
        {
            var top = MirageLayout.Cursor.ScreenPosition.Y;
            DrawListItem(state, entry, indent, reorderEnabled);
            rowBounds.Add((entry.Id, top, top + MirageLayout.Style.FrameHeight));
        }

        if (!reorderEnabled
            || string.IsNullOrEmpty(state.EntryReorderDragId)
            || rowBounds.FindIndex(row => string.Equals(row.Id, state.EntryReorderDragId, StringComparison.Ordinal)) < 0)
            return;

        UpdateEntryReorder(state, rowBounds, scale);
    }

    private static void UpdateEntryReorder(
        MirageTwoColumnState state,
        List<(string Id, float Top, float Bottom)> rows,
        float scale)
    {
        var dragId = state.EntryReorderDragId;
        if (string.IsNullOrEmpty(dragId))
            return;

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var dropIndex = rows.Count == 0
                ? 0
                : ComputeDropInsertIndex(rows, ImGui.GetMousePos().Y);
            var from = rows.FindIndex(row => string.Equals(row.Id, dragId, StringComparison.Ordinal));
            state.EntryReorderDragId = null;
            state.OnEntryReorderDragIdChanged?.Invoke(null);

            if (from >= 0 && dropIndex != from && dropIndex != from + 1)
                state.OnEntryReordered?.Invoke(dragId, dropIndex);
            return;
        }

        if (rows.Count == 0)
            return;

        DrawReorderGapHighlight(rows, ComputeDropInsertIndex(rows, ImGui.GetMousePos().Y), scale);
    }

    private static int ComputeDropInsertIndex(
        List<(string Id, float Top, float Bottom)> rows,
        float mouseY)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var mid = (rows[i].Top + rows[i].Bottom) * 0.5f;
            if (mouseY < mid)
                return i;
        }

        return rows.Count;
    }

    private static void DrawReorderGapHighlight(
        List<(string Id, float Top, float Bottom)> rows,
        int insertIndex,
        float scale)
    {
        if (rows.Count == 0)
            return;

        float y;
        if (insertIndex <= 0)
            y = rows[0].Top;
        else if (insertIndex >= rows.Count)
            y = rows[^1].Bottom;
        else
            y = (rows[insertIndex - 1].Bottom + rows[insertIndex].Top) * 0.5f;

        var padding = 12f * scale;
        var thickness = Math.Max(2f, 3f * scale);
        var window = ImGuiP.GetCurrentWindow();
        var left = window.InnerRect.Min.X + padding;
        var right = window.InnerRect.Max.X - padding;
        var accent = MirageUi.ToUInt(MirageUi.GetColor(MirageUi.Color.Accent));
        ImGui.GetWindowDrawList().AddRectFilled(
            new Vector2(left, y - thickness * 0.5f),
            new Vector2(right, y + thickness * 0.5f),
            accent,
            thickness);
    }

    private static bool HasActiveSearch(MirageTwoColumnState state) =>
        state.ShowSearch && !string.IsNullOrWhiteSpace(state.SearchFilter);

    private static IEnumerable<MirageTwoColumnSidebarNode> GetVisibleSidebarNodes(
        MirageTwoColumnState state,
        bool hasSearch)
    {
        foreach (var node in state.SidebarNodes)
        {
            switch (node)
            {
                case MirageTwoColumnPageNode page:
                    if (!IsEntryVisible(state, hasSearch, page.Entry))
                        continue;

                    yield return page;
                    break;
                case MirageTwoColumnFolderNode folder:
                    if (!IsFolderVisible(state, hasSearch, folder))
                        continue;

                    if (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter))
                    {
                        yield return folder;
                        break;
                    }

                    var matchingEntries = folder.Entries
                        .Where(entry => IsEntryVisible(state, hasSearch: true, entry))
                        .ToList();
                    if (matchingEntries.Count == 0)
                        break;

                    yield return new MirageTwoColumnFolderNode
                    {
                        Id = folder.Id,
                        Label = folder.Label,
                        Entries = matchingEntries,
                        AlwaysExpanded = folder.AlwaysExpanded,
                    };
                    break;
            }
        }
    }

    private static bool IsFolderVisible(MirageTwoColumnState state, bool hasSearch, MirageTwoColumnFolderNode folder)
    {
        if (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter))
            return folder.AlwaysExpanded || folder.Entries.Count > 0;

        if (MirageUi.MatchesFilter(folder.Id, folder.Label, state.SearchFilter))
            return true;

        return folder.Entries.Any(entry => IsEntryVisible(state, hasSearch: true, entry));
    }

    private static bool IsEntryVisible(MirageTwoColumnState state, bool hasSearch, MirageTwoColumnEntry entry)
    {
        if (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter))
            return true;

        return MirageUi.MatchesFilter(entry.Id, entry.Label, state.SearchFilter);
    }

    private static MirageTwoColumnEntry? GetFirstVisibleEntry(MirageTwoColumnState state)
    {
        if (UsesGroupedSidebar(state))
        {
            foreach (var node in GetVisibleSidebarNodes(state, state.ShowSearch))
            {
                switch (node)
                {
                    case MirageTwoColumnPageNode page:
                        return page.Entry;
                    case MirageTwoColumnFolderNode folder when folder.Entries.Count > 0:
                        return folder.Entries[0];
                }
            }

            return null;
        }

        return GetVisibleEntries(state, state.ShowSearch).FirstOrDefault();
    }

    private static IEnumerable<MirageTwoColumnEntry> GetVisibleEntries(
        MirageTwoColumnState state,
        bool hasSearch)
    {
        foreach (var entry in state.Entries)
        {
            if (hasSearch
                && !string.IsNullOrWhiteSpace(state.SearchFilter)
                && !MirageUi.MatchesFilter(entry.Id, entry.Label, state.SearchFilter))
                continue;

            yield return entry;
        }
    }

    private static MirageTwoColumnEntryKind ResolveEntryKind(MirageTwoColumnState state, MirageTwoColumnEntry entry) =>
        entry.Kind != MirageTwoColumnEntryKind.Default
            ? entry.Kind
            : state.ShowEntryToggle ? MirageTwoColumnEntryKind.Bool : MirageTwoColumnEntryKind.Default;

    private static void DrawListItem(
        MirageTwoColumnState state,
        MirageTwoColumnEntry entry,
        float indent = 0f,
        bool reorderEnabled = false)
    {
        using var entryId = ImRaii.PushId(entry.Id);

        ImGui.AlignTextToFramePadding();
        var rowStart = MirageLayout.Cursor.Position;
        var rowHeight = MirageUi.ResolveControlHeight();
        var kind = ResolveEntryKind(state, entry);
        var hasLeadingControl = kind is MirageTwoColumnEntryKind.Bool or MirageTwoColumnEntryKind.Run;
        var isDragSource = reorderEnabled
            && string.Equals(state.EntryReorderDragId, entry.Id, StringComparison.Ordinal);
        var labelIndent = hasLeadingControl ? 0f : indent;

        if (hasLeadingControl)
        {
            if (indent > 0f)
                MirageLayout.Cursor.X += indent;

            switch (kind)
            {
                case MirageTwoColumnEntryKind.Bool:
                {
                    var enabled = entry.Enabled;
                    if (ImGui.Checkbox("##TwoColumnToggle"u8, ref enabled))
                    {
                        entry.Enabled = enabled;
                        state.OnEnabledChanged?.Invoke(entry.Id, enabled);
                    }

                    break;
                }
                case MirageTwoColumnEntryKind.Run:
                {
                    var run = entry.Run;
                    if (run != null)
                    {
                        using var runId = ImRaii.PushId("##Run"u8);
                        ref var isRunning = ref run.IsRunning;
                        ListSelectable.DrawRunButton(ref isRunning, run, rowHeight);
                    }

                    break;
                }
            }

            ImGui.SameLine(0, MirageLayout.Style.ItemInnerSpacing.X);
        }

        var isSelected = state.SelectedId == entry.Id;
        var pressed = ListSelectable.Draw(
            entry.Label,
            isSelected,
            rowHeight,
            labelIndent,
            dimmed: isDragSource,
            labelColor: entry.LabelColor);

        if (reorderEnabled
            && ImGui.IsItemActive()
            && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 5f)
            && !string.Equals(state.EntryReorderDragId, entry.Id, StringComparison.Ordinal))
        {
            state.EntryReorderDragId = entry.Id;
            state.OnEntryReorderDragIdChanged?.Invoke(entry.Id);
        }

        if (pressed && string.IsNullOrEmpty(state.EntryReorderDragId))
        {
            var pluginPageWasVisible = MiragePluginPage.IsVisible;
            MiragePluginPage.Hide();
            if (pluginPageWasVisible && isSelected)
                return;

            string? nextId;
            if (isSelected && state.AllowDeselect)
                nextId = null;
            else
                nextId = entry.Id;

            state.SelectedId = nextId;
            if (nextId != null)
                state.OnSelectionChanged?.Invoke(nextId);
            else
                state.OnSelectionChanged?.Invoke(string.Empty);
        }

        if (entry.ContextMenuItems.Count > 0
            && ImGui.IsItemHovered()
            && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            state.SelectedId = entry.Id;
            MiragePluginPage.Hide();
            state.OnSelectionChanged?.Invoke(entry.Id);
            ImGui.OpenPopup("##EntryContext");
        }

        DrawEntryContextMenu(entry);

        if (isSelected && state.ScrollSelectedIntoView)
        {
            ImGui.SetScrollHereY();
            state.ScrollSelectedIntoView = false;
        }

        MirageLayout.Cursor.Y = rowStart.Y + rowHeight + state.ItemSpacing * MirageLayout.Style.Scale;
    }

    private static void DrawEntryContextMenu(MirageTwoColumnEntry entry)
    {
        if (entry.ContextMenuItems.Count == 0)
            return;

        var labels = new string[entry.ContextMenuItems.Count];
        for (var i = 0; i < entry.ContextMenuItems.Count; i++)
            labels[i] = entry.ContextMenuItems[i].Label;

        var style = MirageContextMenuStyle.CreateDefault();
        if (!MirageUi.ContextMenu.Begin("##EntryContext", labels, style))
            return;

        foreach (var item in entry.ContextMenuItems)
        {
            var icon = item.Icon ?? FontAwesomeIcon.Circle;
            if (!MirageUi.ContextMenu.DrawItem(item.Label, icon, item.Id, style))
                continue;

            item.OnClick?.Invoke();
            ImGui.CloseCurrentPopup();
        }

        MirageUi.ContextMenu.End();
    }

    private static void DrawContentPanels(
        MirageTwoColumnState state,
        float scale,
        Action drawCenterContent)
    {
        var height = MirageLayout.Style.ContentRegionAvail.Y;
        var availWidth = MirageLayout.Style.ContentRegionAvail.X;
        if (MiragePluginPage.IsVisible)
        {
            DrawContentPanel(
                state,
                scale,
                "##TwoColumnMain"u8,
                new Vector2(availWidth, height),
                MiragePluginPage.Draw);
            return;
        }

        DrawContentPanel(
            state,
            scale,
            "##TwoColumnMain"u8,
            new Vector2(availWidth, height),
            drawCenterContent);
    }

    private static void DrawContentPanel(
        MirageTwoColumnState state,
        float scale,
        ReadOnlySpan<byte> id,
        Vector2 size,
        Action drawContent)
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, state.MainPadding * scale);
        using var child = ImRaii.Child(id, size, false, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!child)
            return;

        style.Dispose();
        drawContent();
    }
}
