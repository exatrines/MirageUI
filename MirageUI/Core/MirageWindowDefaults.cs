using System.Runtime.CompilerServices;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;

namespace MirageUI;

public static class MirageWindowDefaults
{
    public static Vector2 DefaultSize { get; } = new(900f, 630f);

    public static Vector2 MaximumSize { get; } = new(4096f, 2160f);

    public static bool Resizable { get; } = false;

    private static readonly ConditionalWeakTable<Window, TitleBarButton> PluginPageButtons = new();
    private static readonly Vector2 PluginPageButtonIconOffset = new(2.5f, 1f);

    public static void ApplyTo(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        window.Size = DefaultSize;
        window.SizeCondition = Resizable ? ImGuiCond.FirstUseEver : ImGuiCond.Always;
        window.SizeConstraints = new()
        {
            MinimumSize = DefaultSize,
            MaximumSize = Resizable ? MaximumSize : DefaultSize,
        };

        if (!Resizable)
            window.Flags |= ImGuiWindowFlags.NoResize;

        EnsurePluginPageButton(window);
    }

    private static void EnsurePluginPageButton(Window window)
    {
        var icon = MirageUi.PluginInfo.TitleBarIcon;
        if (PluginPageButtons.TryGetValue(window, out var existing))
        {
            existing.Icon = icon;
            existing.Priority = 0;
            existing.IconOffset = PluginPageButtonIconOffset;
            return;
        }

        var button = new TitleBarButton
        {
            Icon = icon,
            IconOffset = PluginPageButtonIconOffset,
            Click = _ => MirageUi.OpenPluginPage(),
        };
        window.TitleBarButtons.Add(button);
        PluginPageButtons.Add(window, button);
    }
}
