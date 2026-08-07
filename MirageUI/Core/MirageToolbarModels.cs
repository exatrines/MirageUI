using Dalamud.Interface;

namespace MirageUI;

/// <summary>Filter checklist config for <see cref="MirageUi.IconToolbar"/>.</summary>
public sealed class MirageToolbarFilterConfig
{
    public FontAwesomeIcon Icon { get; set; } = FontAwesomeIcon.Filter;
    public string Id { get; set; } = "toolbarFilter";
    public string? Tooltip { get; set; }
    public IReadOnlyList<MirageChecklistOption> Options { get; set; } = [];
    public HashSet<string> SelectedIds { get; set; } = new(StringComparer.Ordinal);
    public MirageChecklistMode Mode { get; set; } = MirageChecklistMode.Unique;
}

/// <summary>Sort config for <see cref="MirageUi.IconToolbar"/>.</summary>
public sealed class MirageToolbarSortConfig
{
    public FontAwesomeIcon AscIcon { get; set; } = FontAwesomeIcon.SortAmountUp;
    public FontAwesomeIcon DescIcon { get; set; } = FontAwesomeIcon.SortAmountDown;
    public string Id { get; set; } = "toolbarSort";
    public string? AscTooltip { get; set; }
    public string? DescTooltip { get; set; }
    public bool Ascending { get; set; } = true;

    /// <summary>Sort targets. Empty or single target: click toggles direction only.</summary>
    public IReadOnlyList<MirageChecklistOption> Targets { get; set; } = [];

    /// <summary>Selected sort target id when <see cref="Targets"/> has entries.</summary>
    public string SelectedTargetId { get; set; } = string.Empty;
}

/// <summary>Trailing action button for <see cref="MirageUi.IconToolbar"/>.</summary>
public sealed class MirageToolbarAction
{
    public required FontAwesomeIcon Icon { get; init; }
    public required string Id { get; init; }
    public string? Tooltip { get; init; }
    public Action? OnClick { get; init; }
    public Func<bool>? IsActive { get; init; }
    public bool Border { get; init; } = true;
}

/// <summary>State for the default filter / sort / search toolbar row.</summary>
public sealed class MirageIconToolbarState
{
    public bool ShowFilter { get; set; } = true;
    public bool ShowSort { get; set; } = true;
    public bool ShowSearch { get; set; } = true;

    public MirageToolbarFilterConfig Filter { get; set; } = new();
    public MirageToolbarSortConfig Sort { get; set; } = new();

    public string Search { get; set; } = string.Empty;
    public string SearchHint { get; set; } = "Search…";
    public string SearchId { get; set; } = "##toolbarSearch";

    public IList<MirageToolbarAction> Actions { get; set; } = new List<MirageToolbarAction>();
}

/// <summary>Row for <see cref="MirageUi.MultiSelectHighlightList"/>.</summary>
public readonly record struct MirageMultiSelectItem(string Id, string Label, string SelectionKey)
{
    public MirageMultiSelectItem(string id, string label)
        : this(id, label, id)
    {
    }
}
