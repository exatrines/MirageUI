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

    public string? SelectedId { get; set; }

    public string SearchFilter { get; set; } = string.Empty;

    public bool ScrollSelectedIntoView { get; set; }

    public Action<string>? OnSelectionChanged { get; set; }

    public Action<string, bool>? OnEnabledChanged { get; set; }
}
