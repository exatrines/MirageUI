using MirageUI.Layout;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// Three-column shell: left sidebar (same as <see cref="TwoColumn"/>),
    /// center content, and a right panel with the same width as center.
    /// </summary>
    public static class ThreeColumn
    {
        public static void Draw(
            MirageTwoColumnState state,
            Action drawCenterContent,
            Action drawRightContent) =>
            TwoColumnLayout.Draw(state, drawCenterContent, drawRightContent);
    }
}
