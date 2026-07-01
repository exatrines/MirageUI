using MirageUI.Fonts;

namespace MirageUI;

public static partial class MirageUi
{
    public enum FontSize
    {
        Default,
        Large,
    }

    public static IDisposable PushFont(FontSize size) => FontRegistry.Push(size);

    internal static float GetFontSizePt(FontSize size) =>
        size switch
        {
            FontSize.Large => ResolveAppliedColors().GetLargeFontSizePt(),
            _ => ResolveAppliedColors().GetDefaultFontSizePt(),
        };
}
