using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    public enum Color
    {
        Default,
        Secondary,
        Accent,
        Title,
        Warning,
        PanelOverlay,
    }

    public static Vector4 GetColor(Color color) =>
        (MirageTheme.Active ?? ResolveAppliedColors()).GetColor(color);

    public static uint PanelOverlay => ToUInt(GetColor(Color.PanelOverlay));

    internal static uint ToUInt(Vector4 color)
    {
        static uint Pack(float value, byte shift) =>
            (uint)(Math.Clamp(value, 0f, 1f) * 255f + 0.5f) << shift;

        return Pack(color.X, 0) | Pack(color.Y, 8) | Pack(color.Z, 16) | Pack(color.W, 24);
    }
}
