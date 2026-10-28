namespace MirageUI.Layout;

using Dalamud.Interface;

public sealed class MirageTwoColumnTrailingAction
{
    public required string Id { get; init; }

    public required FontAwesomeIcon Icon { get; init; }

    public string? Tooltip { get; init; }

    public Action? OnClick { get; init; }

    public IList<MirageTwoColumnContextMenuItem> ContextMenuItems { get; init; } = [];
}
