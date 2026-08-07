namespace MirageUI.Layout;

public abstract class MirageTwoColumnSidebarNode;

public sealed class MirageTwoColumnFolderNode : MirageTwoColumnSidebarNode
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public IList<MirageTwoColumnEntry> Entries { get; init; } = [];

    /// <summary>When true, the folder stays expanded and its header does not toggle.</summary>
    public bool AlwaysExpanded { get; init; }

    public IList<MirageTwoColumnTrailingAction> TrailingActions { get; init; } = [];
}

public sealed class MirageTwoColumnPageNode : MirageTwoColumnSidebarNode
{
    public required MirageTwoColumnEntry Entry { get; init; }
}
