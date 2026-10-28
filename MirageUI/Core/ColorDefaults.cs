using MirageUI.Theme;

namespace MirageUI;

internal static class ColorDefaults
{
    internal static Vector4 Get(MirageUi.Color color) =>
        color switch
        {
            MirageUi.Color.Secondary => V(0.42f, 0.42f, 0.45f, 1f),
            MirageUi.Color.Accent => V(0.8392157f, 0.41568628f, 0.52156866f, 1f),
            MirageUi.Color.Title => V(0.72f, 0.28f, 0.40f, 1f),
            MirageUi.Color.Warning => V(0.78f, 0.38f, 0.08f, 1f),
            MirageUi.Color.Info => V(0.16f, 0.42f, 0.68f, 1f),
            _ => V(0.16f, 0.16f, 0.18f, 1f),
        };

    internal static void ApplyTo(MirageColorSettings settings)
    {
        foreach (MirageUi.Color color in Enum.GetValues<MirageUi.Color>())
            settings.SetColor(color, Get(color));
    }

    private static Vector4 V(float x, float y, float z, float w) => new(x, y, z, w);
}
