namespace MirageUI.Layout;

public sealed class MirageTwoColumnSidebarHeader
{
    public string? ImagePath { get; init; }

    public float ImageWidth { get; init; } = 48f;

    public float ImageHeight { get; init; } = 48f;

    public bool ImageIsCircle { get; init; }

    public string? Title { get; init; }

    public string? Subtitle { get; init; }

    internal bool HasContent =>
        !string.IsNullOrWhiteSpace(ImagePath)
        || !string.IsNullOrWhiteSpace(Title)
        || !string.IsNullOrWhiteSpace(Subtitle);
}
