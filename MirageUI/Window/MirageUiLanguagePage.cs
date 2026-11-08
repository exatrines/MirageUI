namespace MirageUI;

public static partial class MirageUi
{
    private static Action<string>? _languagePersist;

    public static void BindLanguage(Action<string>? persist) =>
        _languagePersist = persist;

    public static void DrawLanguagePage()
    {
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16f, 12f) * scale);
        var open = ImGui.BeginChild("##MirageLanguagePage", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.AlwaysUseWindowPadding);
        ImGui.PopStyleVar();
        try
        {
            if (open)
                DrawLanguageBody();
        }
        finally
        {
            ImGui.EndChild();
        }
    }

    internal static void ResetLanguage() =>
        _languagePersist = null;

    private static void DrawLanguageBody()
    {
        Text(I18n.Get("language.title"), Color.Title, fontSize: FontSize.Large);
        var labels = new[] { I18n.Get("language.en"), I18n.Get("language.ja") };
        var selected = I18n.IsJapanese ? labels[1] : labels[0];
        if (!Dropdown(I18n.Get("language.title"), ref selected, labels, id: "uiLanguage", allowClear: false))
            return;

        SelectLanguage(selected == labels[1] ? I18n.Japanese : I18n.English);
    }

    public static void SelectLanguage(string language)
    {
        var next = I18n.IsJapaneseCode(language) ? I18n.Japanese : I18n.English;
        if (next == I18n.Language)
            return;

        I18n.SetLanguage(next);
        _languagePersist?.Invoke(next);
    }
}
