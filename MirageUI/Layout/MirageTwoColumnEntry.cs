namespace MirageUI.Layout;

public sealed class MirageTwoColumnEntry
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public MirageTwoColumnEntryKind Kind { get; init; } = MirageTwoColumnEntryKind.Default;

    public bool Enabled { get; set; } = true;

    public MirageTwoColumnRunBinding? Run { get; init; }
}
