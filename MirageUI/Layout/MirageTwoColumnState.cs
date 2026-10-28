namespace MirageUI.Layout;

public sealed class MirageTwoColumnState
{
    public float SidebarWidth { get; init; } = 304f;

    public bool SidebarCollapsed { get; set; }

    public float ItemSpacing { get; init; } = 5f;

    public Vector2 MainPadding { get; init; } = new(16, 12);

    public Vector2 SidebarPadding { get; init; } = new(12, 12);

    public Vector2 SearchFramePadding { get; init; } = new(8, 6);

    public string SearchHint { get; set; } = string.Empty;

    public bool ShowSearch { get; set; }

    public bool ShowEntryToggle { get; set; }

    public bool ShowSidebar { get; set; } = true;

    public int SearchMaxLength { get; init; } = 512;

    public IList<MirageTwoColumnEntry> Entries { get; set; } = [];

    public IList<MirageTwoColumnSidebarNode> SidebarNodes { get; set; } = [];

    public HashSet<string> CollapsedFolderIds { get; set; } = new(StringComparer.Ordinal);

    public float FolderPageIndent { get; init; } = 16f;

    public string? SelectedId { get; set; }

    public string SearchFilter { get; set; } = string.Empty;

    public bool ScrollSelectedIntoView { get; set; }

    public bool AllowDeselect { get; set; } = true;

    public bool AutoSelectFirstOnSearch { get; set; } = true;

    public Action<string>? OnSelectionChanged { get; set; }

    public Action<string>? OnSearchFilterChanged { get; set; }

    public IList<MirageTwoColumnTrailingAction> SearchTrailingActions { get; init; } = [];

    public Action<string, bool>? OnEnabledChanged { get; set; }

    public bool EnableEntryReorder { get; set; }

    public string? EntryReorderDragId { get; set; }

    public Action<string?>? OnEntryReorderDragIdChanged { get; set; }

    public Action<string, int>? OnEntryReordered { get; set; }
}
