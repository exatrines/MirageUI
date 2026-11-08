using System.Text.Json;

namespace MirageUI.Theme;

// Built-in themes plus the host's saved custom themes.
public static class MirageThemes
{
    private const string WhiteId = MirageColorSettings.WhiteThemeId;
    private const string BlackId = MirageColorSettings.BlackThemeId;

    private static readonly JsonSerializerOptions EnvelopeOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static List<MirageSavedTheme> _themes = [];
    private static string _selectedId = WhiteId;
    private static Action? _save;
    private static MirageColorSettings _current = MirageColorSettings.CreateWhite();

    public static MirageColorSettings Current => _current;

    public static string SelectedId => _selectedId;

    public static IReadOnlyList<MirageSavedTheme> All => _themes;

    public static MirageSavedTheme? Selected =>
        _themes.Find(theme => theme.Id == _selectedId);

    public static bool IsBuiltin(string? id) => MirageColorSettings.IsBuiltinTheme(id);

    public static string DisplayName(MirageSavedTheme theme)
    {
        var name = theme.Name;
        if (!IsBuiltin(theme.Id))
            return name;

        return name + MirageUi.I18n.Get("theme.default");
    }

    public static string Bind(List<MirageSavedTheme> themes, string? selectedId, string? legacyId, Action save)
    {
        ArgumentNullException.ThrowIfNull(themes);
        ArgumentNullException.ThrowIfNull(save);

        var customs = themes
            .Where(theme => !IsBuiltin(theme.Id) && !string.IsNullOrWhiteSpace(theme.Id))
            .ToList();
        themes.Clear();
        themes.Add(Builtin(WhiteId, "MirageUI White", MirageColorSettings.CreateWhite()));
        themes.Add(Builtin(BlackId, "MirageUI Black", MirageColorSettings.CreateBlack()));
        themes.AddRange(customs);

        _themes = themes;
        _save = save;
        _selectedId = ResolveId(selectedId, legacyId);
        Apply();
        return _selectedId;
    }

    public static void Select(string id)
    {
        if (_themes.All(theme => theme.Id != id))
            return;

        _selectedId = id;
        Apply();
        _save?.Invoke();
    }

    public static void SaveCurrent()
    {
        var entry = Selected;
        if (entry == null || IsBuiltin(entry.Id))
            return;

        entry.Json = MirageColorSettingsJson.Export(_current);
        _save?.Invoke();
    }

    public static void Add() =>
        AddTheme("Theme", MirageColorSettingsJson.Export(MirageColorSettings.CreateWhite()));

    public static void DuplicateSelected()
    {
        var current = Selected ?? _themes[0];
        AddTheme(current.Name, MirageColorSettingsJson.Export(_current));
    }

    public static void DeleteSelected()
    {
        if (IsBuiltin(_selectedId))
            return;

        var index = _themes.FindIndex(theme => theme.Id == _selectedId);
        if (index < 0)
            return;

        _themes.RemoveAt(index);
        _selectedId = _themes[Math.Max(0, index - 1)].Id;
        Apply();
        _save?.Invoke();
    }

    public static void ExportSelected()
    {
        var entry = Selected;
        if (entry == null)
            return;

        using var colors = JsonDocument.Parse(MirageColorSettingsJson.Export(_current));
        var envelope = new ThemeEnvelope
        {
            Name = entry.Name,
            Theme = colors.RootElement.Clone(),
        };
        ImGui.SetClipboardText(JsonSerializer.Serialize(envelope, EnvelopeOptions));
    }

    public static void ImportClipboard()
    {
        var text = ImGui.GetClipboardText();
        if (string.IsNullOrWhiteSpace(text) || !TryParse(text, out var name, out var themeJson))
        {
            UiContext.Log?.Warning("Theme clipboard is empty or not a theme.");
            return;
        }

        AddTheme(name, themeJson);
    }

    internal static void Reset()
    {
        _themes = [];
        _selectedId = WhiteId;
        _current = MirageColorSettings.CreateWhite();
        _save = null;
    }

    private static void Apply()
    {
        var entry = Selected ?? _themes[0];
        _selectedId = entry.Id;
        if (IsBuiltin(entry.Id))
        {
            _current = MirageColorSettings.CreateBuiltin(entry.Id);
            return;
        }

        if (!MirageColorSettingsJson.TryImport(entry.Json, out var loaded))
            loaded = MirageColorSettings.CreateWhite();
        _current = loaded;
    }

    private static void AddTheme(string name, string json)
    {
        var added = new MirageSavedTheme
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = UniqueName(name),
            Json = json,
        };
        _themes.Add(added);
        _selectedId = added.Id;
        Apply();
        _save?.Invoke();
    }

    private static string ResolveId(string? selectedId, string? legacyId)
    {
        if (Known(selectedId))
            return selectedId!;
        if (legacyId == BlackId)
            return BlackId;
        return WhiteId;
    }

    private static bool Known(string? id) =>
        !string.IsNullOrEmpty(id) && _themes.Exists(theme => theme.Id == id);

    private static MirageSavedTheme Builtin(string id, string name, MirageColorSettings factory) =>
        new()
        {
            Id = id,
            Name = name,
            Json = MirageColorSettingsJson.Export(factory),
        };

    private static string UniqueName(string name)
    {
        var taken = new HashSet<string>(_themes.Select(theme => theme.Name), StringComparer.Ordinal);
        var stem = string.IsNullOrWhiteSpace(name) ? "Theme" : name.Trim();
        if (!taken.Contains(stem))
            return stem;

        var split = stem.LastIndexOf(' ');
        if (split > 0 && int.TryParse(stem[(split + 1)..], out _))
            stem = stem[..split];

        for (var number = 2; ; number++)
        {
            var candidate = $"{stem} {number}";
            if (!taken.Contains(candidate))
                return candidate;
        }
    }

    private static bool TryParse(string text, out string name, out string themeJson)
    {
        name = "";
        themeJson = "";
        try
        {
            var envelope = JsonSerializer.Deserialize<ThemeEnvelope>(text, EnvelopeOptions);
            if (envelope?.Theme.ValueKind == JsonValueKind.Object)
            {
                var raw = envelope.Theme.GetRawText();
                if (!MirageColorSettingsJson.TryImport(raw, out _))
                    return false;

                name = string.IsNullOrWhiteSpace(envelope.Name) ? "Theme" : envelope.Name;
                themeJson = raw;
                return true;
            }
        }
        catch (JsonException)
        {
        }

        if (!MirageColorSettingsJson.TryImport(text, out _))
            return false;

        name = "Theme";
        themeJson = text;
        return true;
    }

    private sealed class ThemeEnvelope
    {
        public string? Name { get; set; }

        public JsonElement Theme { get; set; }
    }
}
