using System.Reflection;
using System.Text.Json;

namespace MirageUI;

public static partial class MirageUi
{
    public static class I18n
    {
        public const string English = "en";
        public const string Japanese = "ja";

        private static readonly Dictionary<string, string> EnglishTable = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> JapaneseTable = new(StringComparer.Ordinal);

        public static string Language { get; private set; } = English;

        public static bool IsJapanese => Language == Japanese;

        public static event Action? LanguageChanged;

        public static void SetLanguage(string? language)
        {
            var next = IsJapaneseCode(language) ? Japanese : English;
            if (next == Language)
                return;

            Language = next;
            LanguageChanged?.Invoke();
        }

        public static string T(string english, string japanese) =>
            IsJapanese && !string.IsNullOrEmpty(japanese) ? japanese : english;

        public static void Register(string language, IReadOnlyDictionary<string, string> entries)
        {
            ArgumentNullException.ThrowIfNull(entries);
            var table = IsJapaneseCode(language) ? JapaneseTable : EnglishTable;
            foreach (var pair in entries)
            {
                if (string.IsNullOrEmpty(pair.Key) || pair.Value == null)
                    continue;

                table[pair.Key] = pair.Value;
            }
        }

        public static void RegisterEmbedded(Assembly assembly, string prefix)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            RegisterResource(assembly, English, prefix + ".en.json");
            RegisterResource(assembly, Japanese, prefix + ".ja.json");
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (IsJapanese && JapaneseTable.TryGetValue(key, out var japanese) && japanese != null)
                return japanese;

            if (EnglishTable.TryGetValue(key, out var english) && english != null)
                return english;

            return key;
        }

        public static string Format(string key, params object?[] args)
        {
            var template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        internal static void Reset()
        {
            EnglishTable.Clear();
            JapaneseTable.Clear();
            Language = English;
            LanguageChanged = null;
        }

        private static void RegisterResource(Assembly assembly, string language, string name)
        {
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing localization resource '{name}'.");
            var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                ?? throw new InvalidOperationException($"Localization resource '{name}' is empty.");
            Register(language, entries);
        }

        private static bool IsJapaneseCode(string? language) =>
            language != null && language.StartsWith(Japanese, StringComparison.OrdinalIgnoreCase);
    }
}
