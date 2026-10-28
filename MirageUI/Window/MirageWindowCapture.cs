using System.Threading.Tasks;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace MirageUI;

public static class MirageWindowCapture
{
    private static string? _windowName;
    private static bool _forPreview;
    private static bool _forCopy;
    private static bool _servicesReady;
    private static IDrawListTextureWrap? _preview;
    private static List<Task> _previewCopies = [];
    private static readonly List<(IDrawListTextureWrap Texture, List<Task> Copies)> Retiring = [];

    internal static IDrawListTextureWrap? Preview => _preview;

    public static void Copy()
    {
        DrainRetired();
        var windowName = _windowName;
        var forPreview = _forPreview;
        var forCopy = _forCopy;
        _windowName = null;
        _forPreview = false;
        _forCopy = false;
        if (windowName == null)
            return;

        IDrawListTextureWrap? texture = null;
        try
        {
            EnsureServices();
            texture = PrintServices.Textures.CreateDrawListTexture("Window image");
            texture.ResizeAndDrawWindow(windowName, Vector2.One);
            if (forPreview)
            {
                Hold(texture);
                texture = null;
            }
            else if (forCopy)
            {
                Watch(PrintServices.Readback.CopyToClipboardAsync(texture, leaveWrapOpen: false));
                texture = null;
            }

            if (forPreview && forCopy)
                CopyHeld();
        }
        catch (Exception ex)
        {
            texture?.Dispose();
            UiContext.Log?.Error(ex, "Failed to capture the window image.");
        }
    }

    public static void Dispose()
    {
        Retire();
        foreach (var (texture, copies) in Retiring)
        {
            if (copies.All(static copy => copy.IsCompleted))
                texture.Dispose();
            else
                _ = Task.WhenAll(copies).ContinueWith(_ => texture.Dispose(), TaskScheduler.Default);
        }

        Retiring.Clear();
    }

    internal static void Request(string windowName, bool preview = false, bool copy = false)
    {
        _windowName = windowName;
        _forPreview |= preview;
        _forCopy |= copy;
    }

    internal static void CopyHeld()
    {
        if (_preview == null)
            return;

        try
        {
            EnsureServices();
            var copy = PrintServices.Readback.CopyToClipboardAsync(_preview, leaveWrapOpen: true);
            _previewCopies.Add(copy);
            Watch(copy);
        }
        catch (Exception ex)
        {
            UiContext.Log?.Error(ex, "Failed to copy the window image.");
        }
    }

    internal static void Retire()
    {
        if (_preview == null)
            return;

        Retiring.Add((_preview, _previewCopies));
        _preview = null;
        _previewCopies = [];
    }

    private static void EnsureServices()
    {
        if (_servicesReady)
            return;

        UiContext.PluginInterface.Create<PrintServices>();
        _servicesReady = true;
    }

    private static void Watch(Task copy)
    {
        _ = copy.ContinueWith(
            task =>
            {
                if (task.Exception != null)
                    UiContext.Log?.Error(task.Exception, "Failed to copy the window image.");
            },
            TaskScheduler.Default);
    }

    private static void Hold(IDrawListTextureWrap texture)
    {
        Retire();
        _preview = texture;
        _previewCopies = [];
    }

    private static void DrainRetired()
    {
        for (var i = Retiring.Count - 1; i >= 0; i--)
        {
            var (texture, copies) = Retiring[i];
            if (copies.Any(static copy => !copy.IsCompleted))
                continue;

            texture.Dispose();
            Retiring.RemoveAt(i);
        }
    }

    private sealed class PrintServices
    {
        [PluginService]
        public static ITextureProvider Textures { get; private set; } = null!;

        [PluginService]
        public static ITextureReadbackProvider Readback { get; private set; } = null!;
    }
}
