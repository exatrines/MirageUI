namespace MirageUI.Layout;

public abstract class MirageTwoColumnSidebarNode;

public sealed class MirageTwoColumnFolderNode : MirageTwoColumnSidebarNode
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public IList<MirageTwoColumnEntry> Entries { get; init; } = [];
}

public sealed class MirageTwoColumnPageNode : MirageTwoColumnSidebarNode
{
    public required MirageTwoColumnEntry Entry { get; init; }
}
