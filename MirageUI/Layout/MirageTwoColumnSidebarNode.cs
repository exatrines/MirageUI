namespace MirageUI.Layout;

public abstract class MirageTwoColumnSidebarNode
{
}

public sealed class MirageTwoColumnPageNode : MirageTwoColumnSidebarNode
{
    public required MirageTwoColumnEntry Entry { get; init; }
}

public sealed class MirageTwoColumnFolderNode : MirageTwoColumnSidebarNode
{
    public required string Id { get; init; }

    public required string Label { get; set; }

    public IList<MirageTwoColumnEntry> Entries { get; init; } = [];

    public bool AlwaysExpanded { get; init; }

    public bool Selectable { get; init; }

    public uint GameIconId { get; init; }
}
