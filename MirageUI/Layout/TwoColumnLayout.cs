namespace MirageUI.Layout;

internal static class TwoColumnLayout
{
    internal static void Draw(
        MirageTwoColumnState state,
        Action drawMainContent)
    {
        var scale = MirageLayout.Style.Scale;
        var startPos = MirageLayout.Cursor.Position;

        if (!state.ShowSidebar)
        {
            MirageLayout.Cursor.Position = startPos;
            DrawMainContent(state, scale, drawMainContent);
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
        DrawMainContent(state, scale, drawMainContent);
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
        return ImGui.GetFontSize() + framePadding.Y * 2f;
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
        var itemSpacing = MirageLayout.Style.ItemSpacing.Y;
        var imageHeight = string.IsNullOrWhiteSpace(header.ImagePath) ? 0f : header.ImageHeight * scale;

        var textHeight = 0f;
        if (!string.IsNullOrWhiteSpace(header.Title))
        {
            using (MirageUi.PushFont(MirageUi.FontSize.Large))
                textHeight += ImGui.CalcTextSize(header.Title).Y;
        }

        if (!string.IsNullOrWhiteSpace(header.Subtitle))
        {
            if (textHeight > 0f)
                textHeight += itemSpacing;

            textHeight += ImGui.CalcTextSize(header.Subtitle).Y;
        }

        var contentHeight = Math.Max(imageHeight, textHeight);
        return padding.Y * 2f + contentHeight + GetPaddedSeparatorHeight();
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
        var hasText = hasTitle || hasSubtitle;

        using var childStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, padding);
        using var child = ImRaii.Child(
            "##TwoColumnSidebarHeader"u8,
            new Vector2(sidebarWidth, headerHeight),
            state.ShowDebugBorders,
            FixedSectionFlags);
        if (!child)
            return;

        childStyle.Dispose();

        if (hasImage)
            MirageUi.Image(header.ImagePath!, imageWidth, imageHeight, header.ImageIsCircle);

        if (hasText)
        {
            var textHeight = 0f;
            if (hasTitle)
            {
                using (MirageUi.PushFont(MirageUi.FontSize.Large))
                    textHeight += ImGui.CalcTextSize(header.Title).Y;
            }

            if (hasSubtitle)
            {
                if (textHeight > 0f)
                    textHeight += MirageLayout.Style.ItemSpacing.Y;

                textHeight += ImGui.CalcTextSize(header.Subtitle).Y;
            }

            if (hasImage)
            {
                var yOffset = Math.Max(0f, (imageHeight - textHeight) * 0.5f);
                ImGui.SameLine(0f, padding.X);
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

        MirageUi.PaddedSeparator();
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

        var searchFilter = state.SearchFilter;
        if (!MirageUi.SearchFilter("##TwoColumnSearchInput"u8, ref searchFilter, state.SearchHint, state.SearchMaxLength))
            return;

        state.SearchFilter = searchFilter;

        var firstEntry = GetFirstVisibleEntry(state);
        var nextId = string.IsNullOrWhiteSpace(state.SearchFilter)
            ? null
            : firstEntry?.Id;
        state.SelectedId = nextId;
        state.OnSelectionChanged?.Invoke(nextId ?? string.Empty);
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
            foreach (var node in GetVisibleSidebarNodes(state, hasSearch))
                DrawSidebarNode(state, node, scale);
            return;
        }

        foreach (var (_, entry) in GetVisibleEntries(state, hasSearch))
            DrawListItem(state, entry);
    }

    private static bool UsesGroupedSidebar(MirageTwoColumnState state) =>
        state.SidebarNodes.Count > 0;

    private static void DrawSidebarNode(
        MirageTwoColumnState state,
        MirageTwoColumnSidebarNode node,
        float scale)
    {
        switch (node)
        {
            case MirageTwoColumnPageNode page:
                DrawListItem(state, page.Entry);
                break;
            case MirageTwoColumnFolderNode folder:
                DrawFolderNode(state, folder, scale);
                break;
        }
    }

    private static void DrawFolderNode(
        MirageTwoColumnState state,
        MirageTwoColumnFolderNode folder,
        float scale)
    {
        using var folderId = ImRaii.PushId(folder.Id);

        var rowStartY = MirageLayout.Cursor.Y;
        var rowHeight = MirageLayout.Style.FrameHeight;
        var forceExpanded = HasActiveSearch(state);
        var expanded = forceExpanded || !state.CollapsedFolderIds.Contains(folder.Id);

        ImGui.AlignTextToFramePadding();
        if (ListSelectable.DrawFolderHeader(folder.Label, expanded, rowHeight))
        {
            if (expanded && !forceExpanded)
                state.CollapsedFolderIds.Add(folder.Id);
            else
                state.CollapsedFolderIds.Remove(folder.Id);
        }

        MirageLayout.Cursor.Y = rowStartY + rowHeight + state.ItemSpacing * scale;

        if (!expanded)
            return;

        var indent = state.FolderPageIndent * scale;
        foreach (var entry in folder.Entries)
            DrawListItem(state, entry, indent);
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
                    };
                    break;
            }
        }
    }

    private static bool IsFolderVisible(MirageTwoColumnState state, bool hasSearch, MirageTwoColumnFolderNode folder)
    {
        if (!hasSearch || string.IsNullOrWhiteSpace(state.SearchFilter))
            return folder.Entries.Count > 0;

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
        float indent = 0f)
    {
        using var entryId = ImRaii.PushId(entry.Id);

        var rowStartY = MirageLayout.Cursor.Y;
        var rowHeight = MirageLayout.Style.FrameHeight;
        var kind = ResolveEntryKind(state, entry);
        var hasLeadingControl = kind is MirageTwoColumnEntryKind.Bool or MirageTwoColumnEntryKind.Run;

        ImGui.AlignTextToFramePadding();

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
        if (ListSelectable.Draw(entry.Label, isSelected, rowHeight, labelIndent))
        {
            var nextId = isSelected ? null : entry.Id;
            state.SelectedId = nextId;
            if (nextId != null)
                state.OnSelectionChanged?.Invoke(nextId);
            else
                state.OnSelectionChanged?.Invoke(string.Empty);
        }

        if (isSelected && state.ScrollSelectedIntoView)
        {
            ImGui.SetScrollHereY();
            state.ScrollSelectedIntoView = false;
        }

        MirageLayout.Cursor.Y = rowStartY + rowHeight + state.ItemSpacing * MirageLayout.Style.Scale;
    }

    private static void DrawMainContent(MirageTwoColumnState state, float scale, Action drawMainContent)
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, state.MainPadding * scale);
        using var child = ImRaii.Child("##TwoColumnMain"u8, new Vector2(-1), false, ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!child)
            return;

        style.Dispose();
        drawMainContent();
    }
}
