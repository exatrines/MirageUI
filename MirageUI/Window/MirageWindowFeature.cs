using Dalamud.Interface;

namespace MirageUI;

public enum MirageWindowFeatureKind
{
    OpenPage,
    Shortcut,
}

public enum MirageWindowRailWhen
{
    Collapsed,
    Expanded,
    Both,
}

public sealed class MirageWindowFeature
{
    public required string Id { get; init; }

    public string? PageId { get; init; }

    public FontAwesomeIcon Icon { get; init; }

    public string? Label { get; init; }

    public string? Tooltip { get; init; }

    public string? OpenPageLabel { get; init; }

    public Action<string>? DrawPopup { get; init; }

    public Func<float>? PopupTextWidth { get; init; }

    public bool ShowOnTitle { get; init; }

    public MirageWindowFeatureKind TitleKind { get; init; }

    public bool TitleOpensPage { get; init; }

    public int TitleOrder { get; init; }

    public bool ShowOnRail { get; init; }

    public MirageWindowRailWhen RailWhen { get; init; } = MirageWindowRailWhen.Both;

    public MirageWindowFeatureKind RailKind { get; init; }

    public bool RailOpensPage { get; init; }

    public int RailOrder { get; init; }
}
