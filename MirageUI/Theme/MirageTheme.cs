using Dalamud.Interface.Utility.Raii;

namespace MirageUI.Theme;

public static class MirageTheme
{
    private static MirageColorSettings? _dalamudDefaults;
    private static MirageColorSettings? _active;
    private static MirageColorSettings? _defaultPreset;

    public static MirageColorSettings? Active => _active;

    public static void EnsureDefaultsCaptured()
    {
        _dalamudDefaults ??= MirageColorSettings.CaptureFromStyle();
    }

    public static MirageColorSettings GetDefault() =>
        _defaultPreset ??= MirageColorSettings.CreateDefault();

    public static MirageColorSettings ResolveAppliedColors() =>
        MirageUi.ResolveAppliedColors();

    public static MirageColorSettings GetEffective(MirageColorSettings settings)
    {
        EnsureDefaultsCaptured();
        return settings.UseCustomColors ? settings : _dalamudDefaults!;
    }

    public static void ResetToDalamudDefaults(MirageColorSettings settings)
    {
        EnsureDefaultsCaptured();
        var defaults = _dalamudDefaults!.Clone();
        defaults.UseCustomColors = false;
        defaults.TitleFontSizePt = MirageColorSettings.DalamudDefaultFontSizePt;
        defaults.UiFontSizePt = MirageColorSettings.DalamudDefaultFontSizePt;
        CopyInto(settings, defaults);
    }

    public static void ResetToDefault(MirageColorSettings settings) =>
        CopyInto(settings, GetDefault());

    public static ImRaii.ColorDisposable PushCustom(MirageColorSettings settings)
    {
        EnsureDefaultsCaptured();
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
        scope.Push(ImGuiCol.TitleBg, settings.TitleBg);
        scope.Push(ImGuiCol.TitleBgActive, settings.TitleBg);
        scope.Push(ImGuiCol.TitleBgCollapsed, settings.TitleBg);
        scope.Push(ImGuiCol.CheckMark, settings.CheckMark);
        return scope;
    }

    public static void Pop(ImRaii.ColorDisposable? scope)
    {
        scope?.Dispose();
        _active = null;
    }

    private static void CopyInto(MirageColorSettings target, MirageColorSettings source)
    {
        target.UseCustomColors = source.UseCustomColors;
        target.WindowBg = source.WindowBg;
        target.ChildBg = source.ChildBg;
        target.Header = source.Header;
        target.HeaderHovered = source.HeaderHovered;
        target.HeaderActive = source.HeaderActive;
        target.FrameBg = source.FrameBg;
        target.FrameBgHovered = source.FrameBgHovered;
        target.Separator = source.Separator;
        target.TitleBg = source.TitleBg;
        target.CheckMark = source.CheckMark;
        target.TitleFontSizePt = source.TitleFontSizePt;
        target.UiFontSizePt = source.UiFontSizePt;
        source.CopyColorsInto(target);
    }
}
