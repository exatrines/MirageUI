namespace MirageUI.Layout;

public sealed class MirageTwoColumnEntry
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public MirageTwoColumnEntryKind Kind { get; init; } = MirageTwoColumnEntryKind.Default;

    public bool Enabled { get; set; } = true;

    public MirageTwoColumnRunBinding? Run { get; init; }

    /// <summary>Optional label tint. Null keeps the default text color.</summary>
    public Vector4? LabelColor { get; init; }

    public IList<MirageTwoColumnTrailingAction> TrailingActions { get; init; } = [];

    public IList<MirageTwoColumnContextMenuItem> ContextMenuItems { get; init; } = [];
}
