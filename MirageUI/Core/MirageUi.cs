using MirageUI.Fonts;
using MirageUI.Theme;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MirageUI;

public static partial class MirageUi
{
    internal static Func<MirageColorSettings> ResolveAppliedColors { get; private set; } =
        MirageColorSettings.CreateDefault;

    public static void ConfigureTheme(Func<MirageColorSettings> resolveAppliedColors) =>
        ResolveAppliedColors = resolveAppliedColors;

    public static void Init(
        IDalamudPluginInterface pluginInterface,
        ITextureProvider? textureProvider = null,
        IPluginLog? log = null)
    {
        UiContext.Set(pluginInterface, log);
        FontRegistry.Init();
        if (textureProvider != null)
            ImageRegistry.Init(textureProvider);
    }

    public static void Dispose()
    {
        ImageRegistry.Dispose();
        FontRegistry.Dispose();
    }
}
