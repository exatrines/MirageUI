using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MirageUI;

internal static class UiContext
{
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    internal static IPluginLog? Log { get; private set; }

    internal static bool IsInitialized { get; private set; }

    internal static IUiBuilder UiBuilder => PluginInterface.UiBuilder;

    internal static void Set(IDalamudPluginInterface pluginInterface, IPluginLog? log = null)
    {
        PluginInterface = pluginInterface ?? throw new ArgumentNullException(nameof(pluginInterface));
        Log = log;
        IsInitialized = true;
    }
}
