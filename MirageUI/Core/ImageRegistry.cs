using System.Collections.Concurrent;
using System.IO;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;

namespace MirageUI;

internal static class ImageRegistry
{
    private static readonly ConcurrentDictionary<string, ISharedImmediateTexture> Textures = new(StringComparer.OrdinalIgnoreCase);

    private static ITextureProvider? _textureProvider;

    internal static void Init(ITextureProvider textureProvider) =>
        _textureProvider = textureProvider ?? throw new ArgumentNullException(nameof(textureProvider));

    internal static bool TryGetWrap(string path, out IDalamudTextureWrap wrap)
    {
        wrap = null!;
        if (_textureProvider == null || string.IsNullOrWhiteSpace(path))
            return false;

        var shared = Textures.GetOrAdd(path, LoadTexture);
        var texture = shared.GetWrapOrDefault();
        if (texture == null)
            return false;

        wrap = texture;
        return true;
    }

    internal static bool TryGetGameIcon(uint iconId, bool hiRes, out IDalamudTextureWrap wrap)
    {
        wrap = null!;
        if (_textureProvider == null || iconId == 0)
            return false;

        var key = $"gameicon:{iconId}:{(hiRes ? "hr" : "lr")}";
        var shared = Textures.GetOrAdd(key, _ =>
            _textureProvider.GetFromGameIcon(new GameIconLookup(iconId, itemHq: false, hiRes: hiRes)));
        var texture = shared.GetWrapOrDefault();
        if (texture == null)
            return false;

        wrap = texture;
        return true;
    }

    internal static void Dispose()
    {
        Textures.Clear();
        _textureProvider = null;
    }

    private static ISharedImmediateTexture LoadTexture(string path) =>
        File.Exists(path)
            ? _textureProvider!.GetFromFile(path)
            : _textureProvider!.GetFromGame(path);
}
