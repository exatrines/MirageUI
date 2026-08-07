namespace MirageUI.Layout;

using Dalamud.Interface;

/// <summary>Icon button drawn on the trailing (right) edge of a sidebar folder, entry, or header.</summary>
public sealed class MirageTwoColumnTrailingAction
{
    public required string Id { get; init; }

    public required FontAwesomeIcon Icon { get; init; }

    public string? Tooltip { get; init; }

    /// <summary>Invoked on click when <see cref="ContextMenuItems"/> is empty.</summary>
    public Action? OnClick { get; init; }

    /// <summary>
    /// When non-empty, left-click opens a context menu instead of invoking <see cref="OnClick"/>.
    /// </summary>
    public IList<MirageTwoColumnContextMenuItem> ContextMenuItems { get; init; } = [];
}
