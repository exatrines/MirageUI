namespace MirageUI.Ui;

internal static class ThemeColors
{
    internal static void TextAccent(string text) => ImGui.TextColored(MirageUi.GetColor(MirageUi.Color.Accent), text);

    internal static void TextSecondary(string text) => ImGui.TextColored(MirageUi.GetColor(MirageUi.Color.Secondary), text);
}
