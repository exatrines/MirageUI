namespace MirageUI.Theme;

using MirageUI;

public sealed class MirageColorSettings
{
    internal const float DefaultFontSizePt = 12f;
    internal const float DefaultLargeFontSizePt = 14f;

    private readonly Vector4[] _colors = new Vector4[ColorCount];

    public bool UseCustomColors;

    public Vector4 WindowBg;
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

    public static MirageColorSettings CreateDefault()
    {
        var settings = new MirageColorSettings
        {
            UseCustomColors = true,
            WindowBg = V(0.11814344f, 0.0662999f, 0.0662999f, 0.93f),
            ChildBg = V(0f, 0f, 0f, 0f),
            Header = V(0.59f, 0.59f, 0.59f, 0.31f),
            HeaderHovered = V(0.5f, 0.5f, 0.5f, 0.8f),
            HeaderActive = V(0.6f, 0.6f, 0.6f, 1f),
            FrameBg = V(0.3628692f, 0.30468765f, 0.30468765f, 0.54f),
            FrameBgHovered = V(1f, 0.9746835f, 0.9746835f, 0.4f),
            Separator = V(0.8392157f, 0.41568628f, 0.52156866f, 0.5f),
            TitleBg = V(0.6329114f, 0.17091277f, 0.28787455f, 0.85067874f),
            CheckMark = V(0.8392157f, 0.41568628f, 0.52156866f, 1f),
            TitleFontSizePt = DefaultLargeFontSizePt,
            UiFontSizePt = DefaultFontSizePt,
        };
        ColorDefaults.ApplyTo(settings);
        return settings;
    }

    private static Vector4 V(float x, float y, float z, float w) => new(x, y, z, w);

    public static MirageColorSettings CaptureFromStyle()
    {
        var colors = ImGui.GetStyle().Colors;
        var settings = new MirageColorSettings
        {
            WindowBg = colors[(int)ImGuiCol.WindowBg],
            ChildBg = colors[(int)ImGuiCol.ChildBg],
            Header = colors[(int)ImGuiCol.Header],
            HeaderHovered = colors[(int)ImGuiCol.HeaderHovered],
            HeaderActive = colors[(int)ImGuiCol.HeaderActive],
            FrameBg = colors[(int)ImGuiCol.FrameBg],
            FrameBgHovered = colors[(int)ImGuiCol.FrameBgHovered],
            Separator = colors[(int)ImGuiCol.Separator],
            TitleBg = colors[(int)ImGuiCol.TitleBg],
            CheckMark = colors[(int)ImGuiCol.CheckMark],
            TitleFontSizePt = DalamudDefaultFontSizePt,
            UiFontSizePt = DalamudDefaultFontSizePt,
        };
        settings.SetColor(MirageUi.Color.Default, colors[(int)ImGuiCol.Text]);
        settings.SetColor(MirageUi.Color.Secondary, colors[(int)ImGuiCol.TextDisabled]);
        settings.SetColor(MirageUi.Color.Title, ColorDefaults.Get(MirageUi.Color.Title));
        settings.SetColor(MirageUi.Color.Accent, ColorDefaults.Get(MirageUi.Color.Accent));
        settings.SetColor(MirageUi.Color.Warning, ColorDefaults.Get(MirageUi.Color.Warning));
        settings.SetColor(MirageUi.Color.PanelOverlay, ColorDefaults.Get(MirageUi.Color.PanelOverlay));
        return settings;
    }

    public MirageColorSettings Clone()
    {
        var clone = new MirageColorSettings
        {
            UseCustomColors = UseCustomColors,
            WindowBg = WindowBg,
            ChildBg = ChildBg,
            Header = Header,
            HeaderHovered = HeaderHovered,
            HeaderActive = HeaderActive,
            FrameBg = FrameBg,
            FrameBgHovered = FrameBgHovered,
            Separator = Separator,
            TitleBg = TitleBg,
            CheckMark = CheckMark,
            TitleFontSizePt = TitleFontSizePt,
            UiFontSizePt = UiFontSizePt,
        };
        CopyColorsInto(clone);
        return clone;
    }

    internal void CopyColorsInto(MirageColorSettings target) =>
        Array.Copy(_colors, target._colors, _colors.Length);
}
