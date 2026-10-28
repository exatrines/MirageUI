using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace MirageUI.Theme;

public static class MirageTheme
{
    private static MirageColorSettings? _active;
    private static ImRaii.StyleDisposable? _scrollbarStyle;

    public static MirageColorSettings? Active => _active;

    public static MirageColorSettings ResolveAppliedColors() =>
        MirageUi.ResolveAppliedColors();

    public static ImRaii.ColorDisposable PushCustom(MirageColorSettings settings)
    {
        _active = settings;

        var scope = new ImRaii.ColorDisposable();
        scope.Push(ImGuiCol.WindowBg, settings.WindowBg);
        scope.Push(ImGuiCol.ChildBg, settings.ChildBg);
        scope.Push(ImGuiCol.Text, MirageUi.GetColor(MirageUi.Color.Default));
        scope.Push(ImGuiCol.TextDisabled, MirageUi.GetColor(MirageUi.Color.Secondary));
        scope.Push(ImGuiCol.Header, settings.Header);
        scope.Push(ImGuiCol.HeaderHovered, settings.HeaderHovered);
        scope.Push(ImGuiCol.HeaderActive, settings.HeaderActive);
        scope.Push(ImGuiCol.FrameBg, settings.FrameBg);
        scope.Push(ImGuiCol.FrameBgHovered, settings.FrameBgHovered);
        scope.Push(ImGuiCol.Separator, settings.Separator);
        scope.Push(ImGuiCol.Border, settings.Separator);
        scope.Push(ImGuiCol.TitleBg, settings.TitleBg);
        scope.Push(ImGuiCol.TitleBgActive, settings.TitleBg);
        scope.Push(ImGuiCol.TitleBgCollapsed, settings.TitleBg);
        scope.Push(ImGuiCol.CheckMark, settings.CheckMark);
        var popup = settings.WindowBg;
        popup.W = 0.96f;
        scope.Push(ImGuiCol.PopupBg, popup);
        var ink = MirageUi.GetColor(MirageUi.Color.Default);
        scope.Push(ImGuiCol.Button, MirageUi.WithAlpha(ink, 0.06f));
        scope.Push(ImGuiCol.ButtonHovered, MirageUi.WithAlpha(ink, 0.10f));
        scope.Push(ImGuiCol.ButtonActive, MirageUi.WithAlpha(ink, 0.16f));
        var accent = settings.GetColor(MirageUi.Color.Accent);
        scope.Push(ImGuiCol.ScrollbarBg, new Vector4(0f, 0f, 0f, 0f));
        scope.Push(ImGuiCol.ScrollbarGrab, new Vector4(0f, 0f, 0f, 0.28f));
        scope.Push(ImGuiCol.ScrollbarGrabHovered, MirageUi.WithAlpha(accent, 0.80f));
        scope.Push(ImGuiCol.ScrollbarGrabActive, accent);
        scope.Push(ImGuiCol.TextSelectedBg, new Vector4(settings.CheckMark.X, settings.CheckMark.Y, settings.CheckMark.Z, 0.28f));

        var scale = ImGuiHelpers.GlobalScale;
        var bar = 14f * scale;
        _scrollbarStyle?.Dispose();
        _scrollbarStyle = new ImRaii.StyleDisposable()
            .Push(ImGuiStyleVar.ScrollbarSize, bar)
            .Push(ImGuiStyleVar.ScrollbarRounding, bar * 0.5f);
        return scope;
    }

    public static void Pop(ImRaii.ColorDisposable? scope)
    {
        _scrollbarStyle?.Dispose();
        _scrollbarStyle = null;
        scope?.Dispose();
        _active = null;
    }
}
