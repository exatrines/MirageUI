using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace MirageUI.Theme;

public static class MirageTheme
{
    private static MirageColorSettings? _active;

    public static MirageColorSettings? Active => _active;

    public static MirageColorSettings ResolveAppliedColors() =>
        MirageUi.ResolveAppliedColors();

    /// <summary>Applies theme colors, the scrollbar, and zero window padding. Dispose restores the previous style.</summary>
    public static IDisposable PushCustom(MirageColorSettings settings)
    {
        var previous = _active;
        _active = settings;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        var colors = new ImRaii.ColorDisposable();
        colors.Push(ImGuiCol.WindowBg, settings.WindowBg);
        colors.Push(ImGuiCol.ChildBg, settings.ChildBg);
        colors.Push(ImGuiCol.Text, MirageUi.GetColor(MirageUi.Color.Default));
        colors.Push(ImGuiCol.TextDisabled, MirageUi.GetColor(MirageUi.Color.Secondary));
        colors.Push(ImGuiCol.Header, settings.Header);
        colors.Push(ImGuiCol.HeaderHovered, settings.HeaderHovered);
        colors.Push(ImGuiCol.HeaderActive, settings.HeaderActive);
        colors.Push(ImGuiCol.FrameBg, settings.FrameBg);
        colors.Push(ImGuiCol.FrameBgHovered, settings.FrameBgHovered);
        colors.Push(ImGuiCol.Separator, settings.Separator);
        colors.Push(ImGuiCol.Border, settings.Separator);
        colors.Push(ImGuiCol.TitleBg, settings.TitleBg);
        colors.Push(ImGuiCol.TitleBgActive, settings.TitleBg);
        colors.Push(ImGuiCol.TitleBgCollapsed, settings.TitleBg);
        colors.Push(ImGuiCol.CheckMark, settings.CheckMark);
        var popup = settings.WindowBg;
        popup.W = 0.96f;
        colors.Push(ImGuiCol.PopupBg, popup);
        var ink = MirageUi.GetColor(MirageUi.Color.Default);
        colors.Push(ImGuiCol.Button, MirageUi.WithAlpha(ink, 0.06f));
        colors.Push(ImGuiCol.ButtonHovered, MirageUi.WithAlpha(ink, 0.10f));
        colors.Push(ImGuiCol.ButtonActive, MirageUi.WithAlpha(ink, 0.16f));
        var accent = settings.GetColor(MirageUi.Color.Accent);
        colors.Push(ImGuiCol.ScrollbarBg, new Vector4(0f, 0f, 0f, 0f));
        colors.Push(ImGuiCol.ScrollbarGrab, new Vector4(0f, 0f, 0f, 0.28f));
        colors.Push(ImGuiCol.ScrollbarGrabHovered, MirageUi.WithAlpha(accent, 0.80f));
        colors.Push(ImGuiCol.ScrollbarGrabActive, accent);
        colors.Push(ImGuiCol.TextSelectedBg, new Vector4(settings.CheckMark.X, settings.CheckMark.Y, settings.CheckMark.Z, 0.28f));

        var scale = ImGuiHelpers.GlobalScale;
        var bar = 14f * scale;
        var style = new ImRaii.StyleDisposable()
            .Push(ImGuiStyleVar.ScrollbarSize, bar)
            .Push(ImGuiStyleVar.ScrollbarRounding, bar * 0.5f);
        return new ThemeScope(previous, colors, style);
    }

    private sealed class ThemeScope : IDisposable
    {
        private readonly MirageColorSettings? _previous;
        private ImRaii.ColorDisposable? _colors;
        private ImRaii.StyleDisposable? _style;
        private bool _disposed;

        public ThemeScope(MirageColorSettings? previous, ImRaii.ColorDisposable colors, ImRaii.StyleDisposable style)
        {
            _previous = previous;
            _colors = colors;
            _style = style;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _style?.Dispose();
            _style = null;
            _colors?.Dispose();
            _colors = null;
            ImGui.PopStyleVar();
            _active = _previous;
        }
    }
}
