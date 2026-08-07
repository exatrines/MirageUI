namespace MirageUI;

using MirageUI.Theme;

/// <summary>Colors and padding for Mirage context menus (ported from EnhancedQuickPanel).</summary>
public sealed class MirageContextMenuStyle
{
    /// <summary>Inner padding inside each menu row.</summary>
    public float Padding { get; set; } = 2f;

    /// <summary>Outer padding around the whole menu content (top/bottom/left/right).</summary>
    public float OuterPadding { get; set; } = 6f;

    public float Width { get; set; } = 160f;

    public Vector4 BgColor { get; set; } = new(0.08f, 0.08f, 0.08f, 0.9f);

    public Vector4 TextColor { get; set; } = new(1f, 1f, 1f, 1f);

    public Vector4 TextHoverColor { get; set; } = new(1f, 1f, 1f, 1f);

    public Vector4 ButtonBgColor { get; set; } = new(0.2f, 0.2f, 0.2f, 1f);

    public Vector4 ButtonBgHoverColor { get; set; } = new(0.31f, 0.31f, 0.31f, 1f);

    public void EnsureDefaults()
    {
        if (Padding < 0f)
            Padding = 0f;

        if (OuterPadding < 0f)
            OuterPadding = 0f;

        if (Width <= 0f)
            Width = 160f;
    }

    public static MirageContextMenuStyle CreateDefault() => new();

    /// <summary>Build a style from the active Mirage theme (falls back to defaults).</summary>
    public static MirageContextMenuStyle FromActiveTheme()
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var text = MirageUi.GetColor(MirageUi.Color.Default);
        return new MirageContextMenuStyle
        {
            Padding = 2f,
            OuterPadding = 6f,
            Width = 160f,
            BgColor = settings.WindowBg,
            TextColor = text,
            TextHoverColor = text,
            ButtonBgColor = settings.Header,
            ButtonBgHoverColor = settings.HeaderHovered,
        };
    }
}
