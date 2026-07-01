using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.ManagedFontAtlas;
using MirageUI;

namespace MirageUI.Fonts;

internal enum FontApplyState
{
    Applied,
    Loading,
    Failed,
}

internal readonly record struct FontApplyStatus(
    FontApplyState State,
    float RequestedSizePt,
    string? ErrorMessage = null);

internal static class FontRegistry
{
    private static IFontAtlas? _atlas;
    private static IFontHandle? _defaultFont;
    private static IFontHandle? _largeFont;
    private static float _requestedDefaultSizePt = -1f;
    private static float _requestedLargeSizePt = -1f;

    public static void Init()
    {
        _atlas = UiContext.UiBuilder.CreateFontAtlas(
            FontAtlasAutoRebuildMode.Async,
            isGlobalScaled: true,
            debugName: "MirageUI.Fonts");
    }

    public static void Dispose()
    {
        _defaultFont?.Dispose();
        _largeFont?.Dispose();
        _defaultFont = null;
        _largeFont = null;
        _requestedDefaultSizePt = -1f;
        _requestedLargeSizePt = -1f;
        _atlas?.Dispose();
        _atlas = null;
    }

    public static IDisposable Push(MirageUi.FontSize size)
    {
        var sizePt = MirageUi.GetFontSizePt(size);
        EnsureFontHandle(size, sizePt);

        var handle = size switch
        {
            MirageUi.FontSize.Large => _largeFont,
            _ => _defaultFont,
        };

        if (handle is { Available: true })
            return handle.Push();

        return NoOpDisposable.Instance;
    }

    public static FontApplyStatus GetApplyStatus(MirageUi.FontSize size)
    {
        var sizePt = MirageUi.GetFontSizePt(size);
        EnsureFontHandle(size, sizePt);

        var handle = size switch
        {
            MirageUi.FontSize.Large => _largeFont,
            _ => _defaultFont,
        };

        return GetApplyStatus(handle, sizePt);
    }

    public static SingleFontSpec BuildFontSpec(float sizePt)
    {
        var baseSpec = UiContext.UiBuilder.DefaultFontSpec as SingleFontSpec
            ?? new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance };

        return baseSpec with { SizePx = (sizePt * 4f) / 3f };
    }

    private static FontApplyStatus GetApplyStatus(IFontHandle? handle, float sizePt)
    {
        if (handle?.LoadException is { } ex)
            return new FontApplyStatus(FontApplyState.Failed, sizePt, ex.Message);

        if (handle is not { Available: true })
            return new FontApplyStatus(FontApplyState.Loading, sizePt);

        return new FontApplyStatus(FontApplyState.Applied, sizePt);
    }

    private static void EnsureFontHandle(MirageUi.FontSize size, float sizePt)
    {
        EnsureAtlas();

        switch (size)
        {
            case MirageUi.FontSize.Large:
                if (_largeFont != null && MathF.Abs(_requestedLargeSizePt - sizePt) < 0.001f)
                    return;

                _largeFont?.Dispose();
                _requestedLargeSizePt = sizePt;
                _largeFont = BuildFontSpec(sizePt).CreateFontHandle(_atlas!);
                break;

            default:
                if (_defaultFont != null && MathF.Abs(_requestedDefaultSizePt - sizePt) < 0.001f)
                    return;

                _defaultFont?.Dispose();
                _requestedDefaultSizePt = sizePt;
                _defaultFont = BuildFontSpec(sizePt).CreateFontHandle(_atlas!);
                break;
        }

        _ = _atlas!.BuildFontsAsync();
    }

    private static void EnsureAtlas()
    {
        if (_atlas == null)
            Init();
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public static readonly NoOpDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
