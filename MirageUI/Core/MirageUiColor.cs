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
        Info,
        PanelOverlay,
    }

    public static Vector4 GetColor(Color color) =>
        (MirageTheme.Active ?? ResolveAppliedColors()).GetColor(color);

    public static uint PanelOverlay => ToUInt(GetColor(Color.PanelOverlay));

    /// <summary>Copy RGB from <paramref name="color"/> and replace alpha.</summary>
    public static Vector4 WithAlpha(Vector4 color, float alpha) =>
        new(color.X, color.Y, color.Z, alpha);

    /// <summary>Linear blend of two colors by <paramref name="t"/> (0–1).</summary>
    public static Vector4 MixColors(Vector4 a, Vector4 b, float t) =>
        new(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t,
            a.W + (b.W - a.W) * t);

    public static uint ToUInt(Vector4 color)
    {
        static uint Pack(float value, byte shift) =>
            (uint)(Math.Clamp(value, 0f, 1f) * 255f + 0.5f) << shift;

        return Pack(color.X, 0) | Pack(color.Y, 8) | Pack(color.Z, 16) | Pack(color.W, 24);
    }
}
