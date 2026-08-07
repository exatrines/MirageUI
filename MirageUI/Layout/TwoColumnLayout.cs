namespace MirageUI.Layout;

using Dalamud.Interface;

internal static class TwoColumnLayout
{
    internal static void Draw(
        MirageTwoColumnState state,
        Action drawMainContent,
        Action? drawRightContent = null)
    {
        var scale = MirageLayout.Style.Scale;
        var startPos = MirageLayout.Cursor.Position;

        if (!state.ShowSidebar)
        {
            MirageLayout.Cursor.Position = startPos;
            DrawContentPanels(state, scale, drawMainContent, drawRightContent);
            return;
        }

        var sidebarWidth = state.SidebarWidth * scale;
        var sidebarContentHeight = MirageLayout.Style.ContentRegionAvail.Y;
        var layout = ComputeSidebarLayout(state, scale, sidebarContentHeight);

        DrawSidebarBackground(sidebarWidth);

        MirageLayout.Cursor.Position = startPos;
        using (var sidebarChild = ImRaii.Child(
            "##TwoColumnSidebar"u8,
            new Vector2(sidebarWidth, sidebarContentHeight),
            state.ShowDebugBorders,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            if (!sidebarChild)
                return;

            if (layout.HeaderHeight > 0f)
            {
                MirageLayout.Cursor.Position = new Vector2(0f, 0f);
                DrawSidebarHeader(state, sidebarWidth, scale, layout.HeaderHeight);
            }

            if (layout.TopSearchHeight > 0f)
            {
                MirageLayout.Cursor.Position = new Vector2(0f, layout.TopSearchY);
                DrawSearch(state, sidebarWidth, scale, atBottom: false, layout.TopSearchHeight);
            }

            MirageLayout.Cursor.Position = new Vector2(layout.ListX, layout.ListTop);
            DrawSidebarList(
                state,
                sidebarWidth,
                scale,
                layout.ListHeight,
                layout.HasSearch);

            if (layout.BottomSearchHeight > 0f)
            {
                MirageLayout.Cursor.Position = new Vector2(0f, layout.BottomSearchY);
                DrawSearch(state, sidebarWidth, scale, atBottom: true, layout.BottomSearchHeight);
            }

            if (layout.FooterHeight > 0f)
            {
                MirageLayout.Cursor.Position = new Vector2(0f, layout.FooterY);
                DrawSidebarFooter(state, sidebarWidth, scale, layout.FooterHeight);
            }
        }

        MirageLayout.Cursor.Position = startPos + new Vector2(sidebarWidth, 0);
        DrawContentPanels(state, scale, drawMainContent, drawRightContent);
    }

    private readonly struct SidebarLayout
    {
        public required float HeaderHeight { get; init; }
        public required float TopSearchY { get; init; }
        public required float TopSearchHeight { get; init; }
        public required float ListX { get; init; }
        public required float ListTop { get; init; }
        public required float ListHeight { get; init; }
        public required float BottomSearchY { get; init; }
        public required float BottomSearchHeight { get; init; }
        public required float FooterY { get; init; }
        public required float FooterHeight { get; init; }
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
        var searchAtBottom = hasSearch && state.SearchPosition == MirageTwoColumnSearchPosition.Bottom;
        var hasSearchAtTop = hasSearch && !searchAtBottom;
        var hasHeader = HasVisibleHeader(state);
        var hasFooter = HasVisibleFooter(state);

        var headerHeight = hasHeader ? GetHeaderHeight(scale, state) : 0f;
        var topSearchHeight = hasSearchAtTop ? GetTopSearchSectionHeight(scale, state) : 0f;
        var bottomSearchHeight = searchAtBottom
            ? GetBottomSearchSectionHeight(scale, state, hasFooter)
            : 0f;
        var footerHeight = hasFooter ? GetFooterHeight(scale, state) : 0f;

        var listTop = headerHeight > 0f ? headerHeight : windowPadding.Y;
        if (topSearchHeight > 0f)
            listTop += topSearchHeight - framePadding.Y;

        var listBottom = contentHeight;
        if (footerHeight > 0f)
            listBottom -= footerHeight;
        if (bottomSearchHeight > 0f)
            listBottom -= bottomSearchHeight;
        else if (footerHeight <= 0f)
            listBottom -= windowPadding.Y;

        var listHeight = Math.Max(0f, listBottom - listTop);
        var topSearchY = headerHeight;
        if (topSearchHeight > 0f && headerHeight <= 0f)
            topSearchY = windowPadding.Y;
        var bottomSearchY = contentHeight - footerHeight - bottomSearchHeight;
        var footerY = contentHeight - footerHeight;

        return new SidebarLayout
        {
            HeaderHeight = headerHeight,
            TopSearchY = topSearchY,
            TopSearchHeight = topSearchHeight,
            ListX = windowPadding.X,
            ListTop = listTop,
            ListHeight = listHeight,
            BottomSearchY = bottomSearchY,
            BottomSearchHeight = bottomSearchHeight,
            FooterY = footerY,
            FooterHeight = footerHeight,
            HasSearch = hasSearch,
        };
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

    private const float BottomSearchListGap = 8f;

    private static float GetBottomSearchTopSpacing(float scale) =>
        BottomSearchListGap * scale;

    private static float GetBottomSearchSectionHeight(float scale, MirageTwoColumnState state, bool hasFooter)
    {
        var inputHeight = GetSearchInputHeight(scale, state);
        var topSpacing = GetBottomSearchTopSpacing(scale);
        var bottomSpacing = hasFooter ? 0f : state.SidebarPadding.Y * scale;
        return topSpacing + inputHeight + bottomSpacing;
    }

    private static void DrawSidebarBackground(float sidebarWidth)
    {
        MirageUi.OverlayFill(
            MirageLayout.Cursor.ScreenPosition,
            new Vector2(sidebarWidth, MirageLayout.Style.ContentRegionAvail.Y),
            MirageLayout.Style.WindowRounding,
            ImDrawFlags.RoundCornersBottomLeft);
    }

    private static bool HasVisibleHeader(MirageTwoColumnState state) =>
        state.ShowSidebarHeader && state.SidebarHeader is { HasContent: true };

    private static bool HasVisibleFooter(MirageTwoColumnState state) =>
        state.ShowSidebarFooter && GetFooterLinks(state).Count > 0;

    private static List<MirageTwoColumnSidebarFooterLink> GetFooterLinks(MirageTwoColumnState state)
    {
        var links = new List<MirageTwoColumnSidebarFooterLink>();
        foreach (var link in state.SidebarFooterLinks)
        {
            if (string.IsNullOrWhiteSpace(link.Label) || string.IsNullOrWhiteSpace(link.Url))
                continue;

            links.Add(link);
        }

        return links;
    }

    private static float GetFooterHeight(float scale, MirageTwoColumnState state)
    {
        var padding = state.SidebarPadding * scale;
        var topSpacing = MirageLayout.Style.ItemSpacing.Y;
        return topSpacing + GetPaddedSeparatorHeight() + MirageUi.GetLinkButtonHeight() + padding.Y;
    }

    private static float GetPaddedSeparatorHeight() =>
        MirageLayout.Style.ItemSpacing.Y * 2f;

    private static float GetHeaderHeight(float scale, MirageTwoColumnState state)
    {
        var header = state.SidebarHeader;
        if (header is not { HasContent: true })
            return 0f;

        var padding = state.SidebarPadding * scale;
        var imageHeight = string.IsNullOrWhiteSpace(header.ImagePath) ? 0f : header.ImageHeight * scale;
        var textHeight = MeasureHeaderTextHeight(header);
        var actionsHeight = header.TrailingActions.Count > 0 ? MirageUi.ResolveControlHeight() : 0f;
        var contentHeight = Math.Max(imageHeight, Math.Max(textHeight, actionsHeight));
        return padding.Y * 2f + contentHeight + GetPaddedSeparatorHeight();
    }

    private static float MeasureHeaderTextHeight(MirageTwoColumnSidebarHeader header)
    {
        var textHeight = 0f;
        if (!string.IsNullOrWhiteSpace(header.Title))
        {
            using (MirageUi.PushFont(MirageUi.FontSize.Large))
                textHeight += ImGui.CalcTextSize(header.Title).Y;
        }

        if (!string.IsNullOrWhiteSpace(header.Subtitle))
        {
            if (textHeight > 0f)
                textHeight += MirageLayout.Style.ItemSpacing.Y;
            textHeight += ImGui.CalcTextSize(header.Subtitle).Y;
        }

        return textHeight;
    }

    private static readonly ImGuiWindowFlags FixedSectionFlags =
        ImGuiWindowFlags.AlwaysUseWindowPadding
        | ImGuiWindowFlags.NoScrollbar
        | ImGuiWindowFlags.NoScrollWithMouse;

    private static void DrawSidebarHeader(
        MirageTwoColumnState state,
        float sidebarWidth,
        float scale,
        float headerHeight)
    {
        var header = state.SidebarHeader!;
        var padding = state.SidebarPadding * scale;
        var imageWidth = header.ImageWidth * scale;
        var imageHeight = header.ImageHeight * scale;
        var hasImage = !string.IsNullOrWhiteSpace(header.ImagePath);
        var hasTitle = !string.IsNullOrWhiteSpace(header.Title);
        var hasSubtitle = !string.IsNullOrWhiteSpace(header.Subtitle);
        var hasActions = header.TrailingActions.Count > 0;
        var hasText = hasTitle || hasSubtitle;
        var buttonSize = MirageUi.ResolveControlHeight();
        var textHeight = MeasureHeaderTextHeight(header);
        var actionsHeight = hasActions ? buttonSize : 0f;
        var contentHeight = Math.Max(imageHeight, Math.Max(textHeight, actionsHeight));

        using var childStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, padding);
        using var child = ImRaii.Child(
            "##TwoColumnSidebarHeader"u8,
            new Vector2(sidebarWidth, headerHeight),
            state.ShowDebugBorders,
            FixedSectionFlags);
        if (!child)
            return;

        childStyle.Dispose();

        var contentStart = MirageLayout.Cursor.Position;

        if (hasImage)
            MirageUi.Image(header.ImagePath!, imageWidth, imageHeight, header.ImageIsCircle);

        if (hasText)
        {
            if (hasImage)
            {
                var yOffset = Math.Max(0f, (imageHeight - textHeight) * 0.5f);
                ImGui.SameLine(0f, padding.X);
                MirageLayout.Cursor.Y += yOffset;
            }
            else
            {
                var yOffset = Math.Max(0f, (contentHeight - textHeight) * 0.5f);
                MirageLayout.Cursor.Y += yOffset;
            }

            ImGui.BeginGroup();
            if (hasTitle)
            {
                using (MirageUi.PushFont(MirageUi.FontSize.Large))
                    MirageUi.Text(header.Title!, MirageUi.Color.Title, wrap: false);
            }

            if (hasSubtitle)
                MirageUi.Text(header.Subtitle!, MirageUi.Color.Secondary, wrap: false);

            ImGui.EndGroup();
        }

        if (hasActions)
            DrawHeaderTrailingActions(header, contentStart.Y + contentHeight - buttonSize, buttonSize);

        MirageLayout.Cursor.Position = new Vector2(contentStart.X, contentStart.Y + contentHeight);
        MirageUi.PaddedSeparator();
    }

    private static void DrawHeaderTrailingActions(
        MirageTwoColumnSidebarHeader header,
        float buttonY,
        float buttonSize)
    {
        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        var actions = header.TrailingActions;
        var actionsWidth = (actions.Count * buttonSize) + ((actions.Count - 1) * gap);
        var startX = MirageLayout.Style.ContentRegionMax.X - actionsWidth;
        MirageLayout.Cursor.Position = new Vector2(startX, buttonY);

        for (var i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            using var actionId = ImRaii.PushId(action.Id);
            if (MirageUi.IconButton(
                    action.Icon,
                    "##headerTrail",
                    new Vector2(buttonSize, buttonSize),
                    tooltip: action.Tooltip))
            {
                if (action.ContextMenuItems.Count > 0)
                    ImGui.OpenPopup("##HeaderTrailContext");
                else
                    action.OnClick?.Invoke();
            }

            DrawTrailingActionContextMenu(action);

            if (i < actions.Count - 1)
                ImGui.SameLine(0, gap);
        }
    }

    private static void DrawTrailingActionContextMenu(MirageTwoColumnTrailingAction action)
    {
        if (action.ContextMenuItems.Count == 0)
            return;

        var labels = new string[action.ContextMenuItems.Count];
        for (var i = 0; i < action.ContextMenuItems.Count; i++)
            labels[i] = action.ContextMenuItems[i].Label;

        var style = MirageContextMenuStyle.CreateDefault();
        if (!MirageUi.ContextMenu.Begin("##HeaderTrailContext", labels, style))
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

    private static void DrawSidebarFooter(
        MirageTwoColumnState state,
        float sidebarWidth,
        float scale,
        float footerHeight)
    {
        var padding = state.SidebarPadding * scale;
        var links = GetFooterLinks(state);

        using var childStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(padding.X, 0f));
        using var child = ImRaii.Child(
            "##TwoColumnSidebarFooter"u8,
            new Vector2(sidebarWidth, footerHeight),
            state.ShowDebugBorders,
            FixedSectionFlags);
        if (!child)
            return;

        childStyle.Dispose();

        MirageLayout.Cursor.Y = MirageLayout.Style.ItemSpacing.Y;
        MirageUi.PaddedSeparator();

        var spacing = Math.Max(MirageLayout.Style.ItemInnerSpacing.X, 8f);
        var linkPaddingX = MirageUi.GetLinkButtonHorizontalPadding();
        var totalWidth = 0f;
        using (MirageUi.PushFont(MirageUi.FontSize.Default))
        {
            for (var i = 0; i < links.Count; i++)
            {
                totalWidth += ImGui.CalcTextSize(links[i].Label).X + linkPaddingX * 2f;
                if (i < links.Count - 1)
                    totalWidth += spacing;
            }
        }

        var contentWidth = sidebarWidth - padding.X * 2f;
        var startX = padding.X + Math.Max(0f, (contentWidth - totalWidth) * 0.5f);
        MirageLayout.Cursor.X = startX;

        for (var i = 0; i < links.Count; i++)
        {
            var link = links[i];
            if (i > 0)
                ImGui.SameLine(0f, spacing);

            using var id = ImRaii.PushId(i);
            MirageUi.Link(link.Label, link.Url);
        }
    }

    private static void DrawSearch(
        MirageTwoColumnState state,
        float sidebarWidth,
        float scale,
        bool atBottom,
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
            state.ShowDebugBorders,
            FixedSectionFlags);
        if (!child)
            return;

        childStyle.Dispose();
        if (atBottom)
            MirageLayout.Cursor.Y += GetBottomSearchTopSpacing(scale);

        using var frameStyle = ImRaii
            .PushStyle(ImGuiStyleVar.FramePadding, framePadding)
            .Push(ImGuiStyleVar.FrameRounding, 3);

        var actions = state.SearchTrailingActions;
        var buttonSize = MirageUi.ResolveControlHeight();
        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        var actionsWidth = actions.Count == 0
            ? 0f
            : (actions.Count * buttonSize) + ((actions.Count - 1) * gap);
        var searchWidth = actionsWidth > 0f
            ? Math.Max(40f, MirageLayout.Style.ContentRegionAvail.X - actionsWidth - gap)
            : -1f;

        var searchFilter = state.SearchFilter;
        var changed = MirageUi.SearchFilter(
            "##TwoColumnSearchInput"u8,
            ref searchFilter,
            state.SearchHint,
            state.SearchMaxLength,
            width: searchWidth);
        state.SearchFilter = searchFilter;
        state.OnSearchFilterChanged?.Invoke(searchFilter);

        if (actions.Count > 0)
        {
            ImGui.SameLine(0f, gap);
            var rowStart = MirageLayout.Cursor.Position;
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

                DrawRowTrailingActionContextMenu(action);
            }

            // Keep layout cursor on the search row baseline.
            _ = rowStart;
        }

        if (!changed || !state.AutoSelectFirstOnSearch)
            return;

        // Keep selection when clearing; jump to first match while filtering.
        if (string.IsNullOrWhiteSpace(state.SearchFilter))
            return;

        var firstEntry = GetFirstVisibleEntry(state);
        if (firstEntry == null)
            return;

        state.SelectedId = firstEntry.Id;
        state.OnSelectionChanged?.Invoke(firstEntry.Id);
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
            state.ShowDebugBorders,
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
                DrawSidebarNode(state, node, scale, reorderEnabled);
            return;
        }

        var visible = GetVisibleEntries(state, hasSearch).ToList();
        var flatReorderEnabled = state.EnableEntryReorder
            && (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter));

        if (!flatReorderEnabled && !string.IsNullOrEmpty(state.EntryReorderDragId))
        {
            state.EntryReorderDragId = null;
            state.OnEntryReorderDragIdChanged?.Invoke(null);
        }

        var rowBounds = new List<(string Id, float Top, float Bottom, int Index)>(visible.Count);

        foreach (var (index, entry) in visible)
        {
            var top = MirageLayout.Cursor.ScreenPosition.Y;
            DrawListItem(state, entry, indent: 0f, flatReorderEnabled);
            var bottom = top + MirageLayout.Style.FrameHeight;
            rowBounds.Add((entry.Id, top, bottom, index));
        }

        if (!flatReorderEnabled)
            return;

        UpdateEntryReorder(state, rowBounds, scale);
    }

    private static void UpdateEntryReorder(
        MirageTwoColumnState state,
        List<(string Id, float Top, float Bottom, int Index)> rows,
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
            var from = rows.FindIndex(r => string.Equals(r.Id, dragId, StringComparison.Ordinal));
            state.EntryReorderDragId = null;
            state.OnEntryReorderDragIdChanged?.Invoke(null);

            if (from >= 0 && dropIndex != from && dropIndex != from + 1)
                state.OnEntryReordered?.Invoke(dragId, dropIndex);
            return;
        }

        if (rows.Count == 0)
            return;

        var insertIndex = ComputeDropInsertIndex(rows, ImGui.GetMousePos().Y);
        DrawReorderGapHighlight(rows, insertIndex, scale);
    }

    private static int ComputeDropInsertIndex(
        List<(string Id, float Top, float Bottom, int Index)> rows,
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
        List<(string Id, float Top, float Bottom, int Index)> rows,
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

    private static bool UsesGroupedSidebar(MirageTwoColumnState state) =>
        state.SidebarNodes.Count > 0;

    private static void DrawSidebarNode(
        MirageTwoColumnState state,
        MirageTwoColumnSidebarNode node,
        float scale,
        bool reorderEnabled)
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

    private static void DrawFolderNode(
        MirageTwoColumnState state,
        MirageTwoColumnFolderNode folder,
        float scale,
        bool reorderEnabled)
    {
        using var folderId = ImRaii.PushId(folder.Id);

        ImGui.AlignTextToFramePadding();
        var rowStart = MirageLayout.Cursor.Position;
        var contentAvailX = MirageLayout.Style.ContentRegionAvail.X;
        var rowHeight = MirageUi.ResolveControlHeight();
        var forceExpanded = HasActiveSearch(state);
        var expanded = folder.AlwaysExpanded
            || forceExpanded
            || !state.CollapsedFolderIds.Contains(folder.Id);
        var trailingWidth = MeasureTrailingActions(folder.TrailingActions, rowHeight);

        if (ListSelectable.DrawFolderHeader(
                folder.Label,
                expanded,
                rowHeight,
                canCollapse: !folder.AlwaysExpanded,
                trailingWidth: trailingWidth)
            && !folder.AlwaysExpanded
            && !forceExpanded)
        {
            if (expanded)
                state.CollapsedFolderIds.Add(folder.Id);
            else
                state.CollapsedFolderIds.Remove(folder.Id);
        }

        DrawTrailingActions(folder.TrailingActions, rowStart, contentAvailX, rowHeight);

        MirageLayout.Cursor.Y = rowStart.Y + rowHeight + state.ItemSpacing * scale;

        if (!expanded)
            return;

        var indent = state.FolderPageIndent * scale;
        var rowBounds = new List<(string Id, float Top, float Bottom, int Index)>(folder.Entries.Count);
        for (var i = 0; i < folder.Entries.Count; i++)
        {
            var entry = folder.Entries[i];
            var top = MirageLayout.Cursor.ScreenPosition.Y;
            DrawListItem(state, entry, indent, reorderEnabled);
            var bottom = top + MirageLayout.Style.FrameHeight;
            rowBounds.Add((entry.Id, top, bottom, i));
        }

        // Reorder only within this folder; ignore drops when drag is from another folder.
        if (!reorderEnabled
            || string.IsNullOrEmpty(state.EntryReorderDragId)
            || rowBounds.FindIndex(r => string.Equals(r.Id, state.EntryReorderDragId, StringComparison.Ordinal)) < 0)
            return;

        UpdateEntryReorder(state, rowBounds, scale);
    }

    private static float MeasureTrailingActions(
        IList<MirageTwoColumnTrailingAction>? actions,
        float rowHeight)
    {
        if (actions == null || actions.Count == 0)
            return 0f;

        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        return (actions.Count * rowHeight) + ((actions.Count - 1) * gap) + gap;
    }

    private static void DrawTrailingActions(
        IList<MirageTwoColumnTrailingAction>? actions,
        Vector2 rowStart,
        float contentAvailX,
        float rowHeight)
    {
        if (actions == null || actions.Count == 0)
            return;

        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        var totalWidth = MeasureTrailingActions(actions, rowHeight);
        // Window-local coords (same space as Cursor.Position / ContentRegionAvail).
        var startX = rowStart.X + contentAvailX - 12f - totalWidth + gap;
        MirageLayout.Cursor.Position = new Vector2(startX, rowStart.Y);

        for (var i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            using var actionId = ImRaii.PushId(action.Id);
            if (MirageUi.IconButton(
                    action.Icon,
                    "##trail",
                    new Vector2(rowHeight, rowHeight),
                    tooltip: action.Tooltip))
            {
                if (action.ContextMenuItems.Count > 0)
                    ImGui.OpenPopup("##TrailContext");
                else
                    action.OnClick?.Invoke();
            }

            DrawRowTrailingActionContextMenu(action);

            if (i < actions.Count - 1)
                ImGui.SameLine(0, gap);
        }
    }

    private static void DrawRowTrailingActionContextMenu(MirageTwoColumnTrailingAction action)
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
                        TrailingActions = folder.TrailingActions,
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

        return GetVisibleEntries(state, state.ShowSearch).Select(pair => pair.Entry).FirstOrDefault();
    }

    private static IEnumerable<(int Index, MirageTwoColumnEntry Entry)> GetVisibleEntries(
        MirageTwoColumnState state,
        bool hasSearch)
    {
        for (var i = 0; i < state.Entries.Count; i++)
        {
            var entry = state.Entries[i];
            if (hasSearch
                && !string.IsNullOrWhiteSpace(state.SearchFilter)
                && !MirageUi.MatchesFilter(entry.Id, entry.Label, state.SearchFilter))
                continue;

            yield return (i, entry);
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
        var contentAvailX = MirageLayout.Style.ContentRegionAvail.X;
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

        var trailingWidth = MeasureTrailingActions(entry.TrailingActions, rowHeight);
        var isSelected = state.SelectedId == entry.Id;
        var pressed = ListSelectable.Draw(
            entry.Label,
            isSelected,
            rowHeight,
            labelIndent,
            trailingWidth,
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

        // Ignore click-to-select once a reorder drag has started.
        if (pressed && string.IsNullOrEmpty(state.EntryReorderDragId))
        {
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
            state.OnSelectionChanged?.Invoke(entry.Id);
            ImGui.OpenPopup("##EntryContext");
        }

        DrawEntryContextMenu(entry);
        DrawTrailingActions(entry.TrailingActions, rowStart, contentAvailX, rowHeight);

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
        Action drawCenterContent,
        Action? drawRightContent)
    {
        var height = MirageLayout.Style.ContentRegionAvail.Y;
        var availWidth = MirageLayout.Style.ContentRegionAvail.X;
        if (drawRightContent == null)
        {
            DrawContentPanel(
                state,
                scale,
                "##TwoColumnMain"u8,
                new Vector2(availWidth, height),
                drawCenterContent);
            return;
        }

        // Center and right share the remaining width equally.
        var panelWidth = availWidth * 0.5f;
        var panelStart = MirageLayout.Cursor.Position;
        DrawContentPanel(
            state,
            scale,
            "##ThreeColumnCenter"u8,
            new Vector2(panelWidth, height),
            drawCenterContent);
        MirageLayout.Cursor.Position = panelStart + new Vector2(panelWidth, 0f);
        DrawContentPanel(
            state,
            scale,
            "##ThreeColumnRight"u8,
            new Vector2(Math.Max(1f, availWidth - panelWidth), height),
            drawRightContent);
    }

    private static void DrawContentPanel(
        MirageTwoColumnState state,
        float scale,
        ReadOnlySpan<byte> id,
        Vector2 size,
        Action drawContent)
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, state.MainPadding * scale);
        using var child = ImRaii.Child(id, size, state.ShowDebugBorders, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!child)
            return;

        style.Dispose();
        drawContent();
    }
}
