using MirageUI.Theme;

namespace MirageUI;

internal static class ColorDefaults
{
    internal static Vector4 Get(MirageUi.Color color) =>
        color switch
        {
            MirageUi.Color.Secondary => V(0.5864979f, 0.5864979f, 0.5864979f, 1f),
            MirageUi.Color.Accent => V(0.8392157f, 0.41568628f, 0.52156866f, 1f),
            MirageUi.Color.Title => V(0.83966243f, 0.4145169f, 0.5221488f, 1f),
            MirageUi.Color.Warning => V(1f, 0.55f, 0.2f, 1f),
            MirageUi.Color.Info => V(0.4f, 0.72f, 0.95f, 1f),
            MirageUi.Color.PanelOverlay => V(1f, 1f, 1f, 0.05f),
            _ => V(1f, 1f, 1f, 1f),
        };

    internal static void ApplyTo(MirageColorSettings settings)
    {
        foreach (MirageUi.Color color in Enum.GetValues<MirageUi.Color>())
            settings.SetColor(color, Get(color));
    }

    private static Vector4 V(float x, float y, float z, float w) => new(x, y, z, w);
}
