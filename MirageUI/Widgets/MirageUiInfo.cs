using Dalamud.Interface;

namespace MirageUI;

public static partial class MirageUi
{
    // Inline info marker. Same control as the title-bar menu.
    public static void InfoButton(string tooltip, string id)
    {
        if (string.IsNullOrEmpty(tooltip))
            return;

        var frame = ImGui.GetFrameHeight();
        ImGui.SameLine(0f, ImGui.GetStyle().ItemInnerSpacing.X);
        IconButton(
            FontAwesomeIcon.InfoCircle,
            id: id,
            size: new Vector2(frame, frame),
            color: GetColor(Color.Secondary),
            tooltip: tooltip);
    }
}
