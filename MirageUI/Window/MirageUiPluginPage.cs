using Dalamud.Utility;

namespace MirageUI;

public static partial class MirageUi
{
    private static Action? _pluginPageDraw;

    public static MiragePluginInfo PluginInfo { get; private set; } = new();

    public static void ConfigurePluginInfo(Action<MiragePluginInfo> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(PluginInfo);
    }

    public static void SetPluginPage(Action? draw) =>
        _pluginPageDraw = draw;

    public static void TogglePluginPage() =>
        MiragePluginPage.Toggle();

    public static void OpenPluginPage(bool focusSupport = false) =>
        MiragePluginPage.OpenCurrent(focusSupport);

    public static void DrawPluginPage()
    {
        if (_pluginPageDraw != null)
        {
            _pluginPageDraw();
            return;
        }

        MiragePluginPage.Draw();
    }

    public static void OpenSupport()
    {
        var url = PluginInfo.SupportUrl;
        if (!string.IsNullOrWhiteSpace(url))
            Util.OpenLink(url);
    }

    internal static void InitPluginPage() =>
        PluginInfo = MiragePluginInfo.FromPluginInterface(UiContext.PluginInterface);

    internal static void DisposePluginPage()
    {
        MiragePluginPage.Dispose();
        _pluginPageDraw = null;
        PluginInfo = new();
    }
}
