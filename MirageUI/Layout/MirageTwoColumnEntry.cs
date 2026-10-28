namespace MirageUI.Layout;

using Dalamud.Interface;

public sealed class MirageTwoColumnEntry
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public FontAwesomeIcon Icon { get; init; } = FontAwesomeIcon.Circle;

    public MirageTwoColumnEntryKind Kind { get; init; } = MirageTwoColumnEntryKind.Default;

    public bool Enabled { get; set; } = true;

    public MirageTwoColumnRunBinding? Run { get; init; }

    public Vector4? LabelColor { get; init; }

    public IList<MirageTwoColumnContextMenuItem> ContextMenuItems { get; init; } = [];
}
