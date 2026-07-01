using MirageUI.Layout;

namespace MirageUI;

public static partial class MirageUi
{
    public static class TwoColumn
    {
        public static void Draw(MirageTwoColumnState state, Action drawMainContent) =>
            TwoColumnLayout.Draw(state, drawMainContent);
    }
}
