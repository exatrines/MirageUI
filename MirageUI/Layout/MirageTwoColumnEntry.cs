namespace MirageUI.Layout;

public sealed class MirageTwoColumnEntry
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public bool Enabled { get; set; } = true;
}
