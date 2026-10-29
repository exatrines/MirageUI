namespace MirageUI.Theme;

using MirageUI;

public sealed class MirageColorSettings
{
    internal const float DefaultFontSizePt = 12f;
    internal const float DefaultLargeFontSizePt = 14f;
    public const float DefaultBlurStrength = 8f;
    public const float MaxBlurStrength = 20f;

    private readonly Vector4[] _colors = new Vector4[ColorCount];

    public bool UseCustomColors;

    public Vector4 WindowBg;
    public Vector4 ChromeBg;
    public Vector4 PageBg;
    public Vector4 ChildBg;

    public Vector4 Header;
    public Vector4 HeaderHovered;
    public Vector4 HeaderActive;

    public Vector4 FrameBg;
    public Vector4 FrameBgHovered;

    public Vector4 Separator;
    public Vector4 TitleBg;
    public Vector4 CheckMark;

    public float TitleFontSizePt = DefaultLargeFontSizePt;

    public float UiFontSizePt = DefaultFontSizePt;

    public float BlurStrength = DefaultBlurStrength;

    internal static int ColorCount => Enum.GetValues<MirageUi.Color>().Length;

    public MirageColorSettings() => ColorDefaults.ApplyTo(this);

    internal static float DalamudDefaultFontSizePt =>
        UiContext.IsInitialized
            ? UiContext.UiBuilder.DefaultFontSpec.SizePt
            : DefaultFontSizePt;

    public float GetLargeFontSizePt() =>
        TitleFontSizePt > 0f ? TitleFontSizePt : DalamudDefaultFontSizePt;

    public float GetDefaultFontSizePt() =>
        UiFontSizePt > 0f ? UiFontSizePt : DalamudDefaultFontSizePt;

    public Vector4 GetColor(MirageUi.Color color) => _colors[(int)color];

    public void SetColor(MirageUi.Color color, Vector4 value) => _colors[(int)color] = value;

    public static MirageColorSettings CreateDefault() => CreateWhite();

    public static MirageColorSettings CreateWhite()
    {
        var settings = new MirageColorSettings
        {
            UseCustomColors = true,
            WindowBg = V(1f, 1f, 1f, 0.392157f),
            ChromeBg = V(1f, 1f, 1f, 0.313725f),
            PageBg = V(1f, 1f, 1f, 0.470588f),
            ChildBg = V(0f, 0f, 0f, 0f),
            FrameBg = V(0f, 0f, 0f, 0.05f),
            FrameBgHovered = V(0f, 0f, 0f, 0.08f),
            Separator = V(0f, 0f, 0f, 0.156863f),
            TitleBg = V(1f, 1f, 1f, 0.60f),
            TitleFontSizePt = DefaultLargeFontSizePt,
            UiFontSizePt = DefaultFontSizePt,
            BlurStrength = DefaultBlurStrength,
        };
        ColorDefaults.ApplyTo(settings);
        settings.ApplyAccent(settings.GetColor(MirageUi.Color.Accent));
        return settings;
    }

    public static MirageColorSettings CreateBlack()
    {
        var settings = new MirageColorSettings
        {
            UseCustomColors = true,
            WindowBg = V(0.14f, 0.14f, 0.16f, 0.392157f),
            ChromeBg = V(0.14f, 0.14f, 0.16f, 0.313725f),
            PageBg = V(0.14f, 0.14f, 0.16f, 0.470588f),
            ChildBg = V(0f, 0f, 0f, 0f),
            FrameBg = V(1f, 1f, 1f, 0.08f),
            FrameBgHovered = V(1f, 1f, 1f, 0.12f),
            Separator = V(1f, 1f, 1f, 0.156863f),
            TitleBg = V(0.14f, 0.14f, 0.16f, 0.78f),
            TitleFontSizePt = DefaultLargeFontSizePt,
            UiFontSizePt = DefaultFontSizePt,
            BlurStrength = DefaultBlurStrength,
        };
        ColorDefaults.ApplyTo(settings);
        settings.SetColor(MirageUi.Color.Default, V(0.94f, 0.94f, 0.95f, 1f));
        settings.SetColor(MirageUi.Color.Secondary, V(0.68f, 0.68f, 0.72f, 1f));
        settings.SetColor(MirageUi.Color.Title, V(0.93f, 0.62f, 0.72f, 1f));
        settings.ApplyAccent(settings.GetColor(MirageUi.Color.Accent));
        return settings;
    }

    public void ApplyAccent(Vector4 accent)
    {
        accent.W = 1f;
        SetColor(MirageUi.Color.Accent, accent);
        CheckMark = accent;
        Header = MirageUi.WithAlpha(accent, 0.16f);
        HeaderHovered = MirageUi.WithAlpha(accent, 0.28f);
        HeaderActive = MirageUi.WithAlpha(accent, 0.40f);
    }

    private static Vector4 V(float x, float y, float z, float w) => new(x, y, z, w);
}
