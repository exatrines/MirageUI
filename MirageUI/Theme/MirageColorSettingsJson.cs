using System.Text.Json;
using System.Text.Json.Serialization;

namespace MirageUI.Theme;

public static class MirageColorSettingsJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Export(MirageColorSettings settings) =>
        JsonSerializer.Serialize(MirageColorSettingsDto.From(settings), Options);

    public static bool TryImport(string json, out MirageColorSettings settings)
    {
        settings = null!;

        if (string.IsNullOrWhiteSpace(json))
        {
            LogImportError("Theme JSON is empty.");
            return false;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<MirageColorSettingsDto>(json, Options);
            if (dto == null)
            {
                LogImportError("Failed to deserialize theme JSON.");
                return false;
            }

            settings = dto.ToSettings();
            settings.UseCustomColors = true;
            return true;
        }
        catch (JsonException ex)
        {
            LogImportError($"Invalid theme JSON format: {ex.Message}");
            return false;
        }
    }

    private static void LogImportError(string message)
    {
        UiContext.Log?.Error($"[MirageUI] {message}");
    }

    private sealed class MirageColorSettingsDto
    {
        public bool? UseCustomColors { get; set; }

        public float[]? WindowBg { get; set; }
        public float[]? ChildBg { get; set; }

        public float[]? Header { get; set; }
        public float[]? HeaderHovered { get; set; }
        public float[]? HeaderActive { get; set; }

        public float[]? FrameBg { get; set; }
        public float[]? FrameBgHovered { get; set; }

        public float[]? Separator { get; set; }
        public float[]? TitleBg { get; set; }
        public float[]? CheckMark { get; set; }

        public float[]? Default { get; set; }
        public float[]? Secondary { get; set; }
        public float[]? Accent { get; set; }
        public float[]? Title { get; set; }
        public float[]? Warning { get; set; }
        public float[]? PanelOverlay { get; set; }

        public float[]? Text { get; set; }
        public float[]? TextDisabled { get; set; }
        public float[]? AccentTitle { get; set; }
        public float[]? AccentEnabled { get; set; }
        public float[]? SidebarOverlay { get; set; }

        public float? TitleFontSizePt { get; set; }
        public float? ToolTitleFontSizePt { get; set; }

        public float? UiFontSizePt { get; set; }

        public static MirageColorSettingsDto From(MirageColorSettings settings) =>
            new()
            {
                UseCustomColors = settings.UseCustomColors,
                WindowBg = ToArray(settings.WindowBg),
                ChildBg = ToArray(settings.ChildBg),
                Header = ToArray(settings.Header),
                HeaderHovered = ToArray(settings.HeaderHovered),
                HeaderActive = ToArray(settings.HeaderActive),
                FrameBg = ToArray(settings.FrameBg),
                FrameBgHovered = ToArray(settings.FrameBgHovered),
                Separator = ToArray(settings.Separator),
                TitleBg = ToArray(settings.TitleBg),
                CheckMark = ToArray(settings.CheckMark),
                Default = ToArray(settings.GetColor(MirageUi.Color.Default)),
                Secondary = ToArray(settings.GetColor(MirageUi.Color.Secondary)),
                Accent = ToArray(settings.GetColor(MirageUi.Color.Accent)),
                Title = ToArray(settings.GetColor(MirageUi.Color.Title)),
                Warning = ToArray(settings.GetColor(MirageUi.Color.Warning)),
                PanelOverlay = ToArray(settings.GetColor(MirageUi.Color.PanelOverlay)),
                TitleFontSizePt = settings.TitleFontSizePt,
                UiFontSizePt = settings.UiFontSizePt,
            };

        public MirageColorSettings ToSettings()
        {
            var settings = new MirageColorSettings
            {
                UseCustomColors = UseCustomColors ?? true,
                WindowBg = ToVector4(WindowBg),
                ChildBg = ToVector4(ChildBg),
                Header = ToVector4(Header),
                HeaderHovered = ToVector4(HeaderHovered),
                HeaderActive = ToVector4(HeaderActive),
                FrameBg = ToVector4(FrameBg),
                FrameBgHovered = ToVector4(FrameBgHovered),
                Separator = ToVector4(Separator),
                TitleBg = ToVector4(TitleBg),
                CheckMark = ToVector4(CheckMark),
                TitleFontSizePt = TitleFontSizePt ?? ToolTitleFontSizePt ?? MirageColorSettings.DefaultLargeFontSizePt,
                UiFontSizePt = UiFontSizePt ?? MirageColorSettings.DefaultFontSizePt,
            };

            settings.SetColor(MirageUi.Color.Default, ToVector4(Default ?? Text));
            settings.SetColor(MirageUi.Color.Secondary, ToVector4(Secondary ?? TextDisabled));
            settings.SetColor(MirageUi.Color.Accent, ToVector4(Accent ?? AccentEnabled));
            settings.SetColor(MirageUi.Color.Title, ToVector4(Title ?? AccentTitle));
            settings.SetColor(MirageUi.Color.Warning, ToVector4(Warning));
            settings.SetColor(MirageUi.Color.PanelOverlay, ToVector4(PanelOverlay ?? SidebarOverlay));

            return settings;
        }

        private static float[] ToArray(Vector4 color) => [color.X, color.Y, color.Z, color.W];

        private static Vector4 ToVector4(float[]? values) =>
            values is { Length: >= 3 }
                ? new Vector4(values[0], values[1], values[2], values.Length > 3 ? values[3] : 1f)
                : default;
    }
}
