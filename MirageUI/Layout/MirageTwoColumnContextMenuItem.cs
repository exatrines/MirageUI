namespace MirageUI.Layout;

using Dalamud.Interface;

public sealed class MirageTwoColumnContextMenuItem
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public FontAwesomeIcon? Icon { get; init; }

    public Action? OnClick { get; init; }
}
