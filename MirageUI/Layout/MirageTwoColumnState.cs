namespace MirageUI.Layout;

public enum MirageTwoColumnSearchPosition
{
    Top,
    Bottom,
}

public sealed class MirageTwoColumnState
{
    public float SidebarWidth { get; init; } = 304f;

    public float ItemSpacing { get; init; } = 5f;

    public Vector2 MainPadding { get; init; } = new(16, 12);

    public Vector2 SidebarPadding { get; init; } = new(12, 12);

    public Vector2 SearchFramePadding { get; init; } = new(8, 6);

    public string SearchHint { get; init; } = string.Empty;

    public MirageTwoColumnSearchPosition SearchPosition { get; set; } = MirageTwoColumnSearchPosition.Top;

    public bool ShowSearch { get; set; }

    public bool ShowEntryToggle { get; set; }

    public bool ShowSidebar { get; set; } = true;

    public bool ShowSidebarHeader { get; set; } = true;

    public bool ShowSidebarFooter { get; set; }

    public IList<MirageTwoColumnSidebarFooterLink> SidebarFooterLinks { get; set; } = [];

    public MirageTwoColumnSidebarHeader? SidebarHeader { get; init; }

    public int SearchMaxLength { get; init; } = 512;

    public bool ShowDebugBorders { get; init; }

    public IList<MirageTwoColumnEntry> Entries { get; set; } = [];

    public IList<MirageTwoColumnSidebarNode> SidebarNodes { get; set; } = [];

    public HashSet<string> CollapsedFolderIds { get; set; } = new(StringComparer.Ordinal);

    public float FolderPageIndent { get; init; } = 16f;

    public string? SelectedId { get; set; }

    public string SearchFilter { get; set; } = string.Empty;

    public bool ScrollSelectedIntoView { get; set; }

    /// <summary>When false, clicking the already-selected entry keeps it selected.</summary>
    public bool AllowDeselect { get; set; } = true;

    /// <summary>
    /// When true (default), changing the search filter selects the first visible entry.
    /// Clearing the filter does not clear the current selection.
    /// </summary>
    public bool AutoSelectFirstOnSearch { get; set; } = true;

    public Action<string>? OnSelectionChanged { get; set; }

    public Action<string>? OnSearchFilterChanged { get; set; }

    /// <summary>Icon buttons drawn to the right of the search field (same row).</summary>
    public IList<MirageTwoColumnTrailingAction> SearchTrailingActions { get; init; } = [];

    public Action<string, bool>? OnEnabledChanged { get; set; }

    /// <summary>Enable drag-and-drop reorder for flat <see cref="Entries"/> or within each folder in <see cref="SidebarNodes"/>.</summary>
    public bool EnableEntryReorder { get; set; }

    /// <summary>Host-persisted id of the entry currently being dragged; null when idle.</summary>
    public string? EntryReorderDragId { get; set; }

    public Action<string?>? OnEntryReorderDragIdChanged { get; set; }

    /// <summary>
    /// Invoked on drop: dragged entry id and insert index in the active list
    /// (flat <see cref="Entries"/>, or the folder that owns the drag when using <see cref="SidebarNodes"/>).
    /// 0 = before first, Count = after last. Not a swap — caller should move/insert.
    /// Cross-folder drops are ignored.
    /// </summary>
    public Action<string, int>? OnEntryReordered { get; set; }
}
