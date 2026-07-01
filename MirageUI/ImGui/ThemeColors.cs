namespace MirageUI.Ui;

internal static class ThemeColors
{
    internal static void TextTitle(string text) => ImGui.TextColored(MirageUi.GetColor(MirageUi.Color.Title), text);

    internal static void TextAccent(string text) => ImGui.TextColored(MirageUi.GetColor(MirageUi.Color.Accent), text);

    internal static void TextSecondary(string text) => ImGui.TextColored(MirageUi.GetColor(MirageUi.Color.Secondary), text);

    internal static void TextWarning(string text) => ImGui.TextColored(MirageUi.GetColor(MirageUi.Color.Warning), text);
}
