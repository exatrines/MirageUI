using Dalamud.Interface.Windowing;

namespace MirageUI;

public static class MirageWindowDefaults
{
    public static Vector2 DefaultSize { get; } = new(900f, 630f);

    public static Vector2 MaximumSize { get; } = new(4096f, 2160f);

    public static bool Resizable { get; } = false;

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
    }
}
