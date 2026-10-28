using Dalamud.Interface;

namespace MirageUI;

public static partial class MirageUi
{
    public static bool ListRow(string title, string detail, ref bool value, string id = "")
    {
        var rowId = string.IsNullOrEmpty(id) ? title : id;
        var trailing = ImGui.GetFrameHeight();
        var clicked = DrawListRow(rowId, title, detail, trailing, out var origin, out var rowWidth, out var rowHeight);
        if (clicked)
            value = !value;

        var after = ImGui.GetCursorScreenPos();
        var padX = 10f * MirageLayout.Style.Scale;
        ImGui.SetCursorScreenPos(new Vector2(
            origin.X + rowWidth - padX - trailing,
            origin.Y + (rowHeight - trailing) * 0.5f));
        var changed = ImGui.Checkbox($"##{rowId}", ref value);
        ImGui.SetCursorScreenPos(after);
        return clicked || changed;
    }

    public static bool ListRow(
        string title,
        string detail,
        FontAwesomeIcon icon,
        string id,
        string? tooltip = null)
    {
        var trailing = ResolveControlHeight();
        DrawListRow(id, title, detail, trailing, out var origin, out var rowWidth, out var rowHeight);

        var after = ImGui.GetCursorScreenPos();
        var padX = 10f * MirageLayout.Style.Scale;
        ImGui.SetCursorScreenPos(new Vector2(
            origin.X + rowWidth - padX - trailing,
            origin.Y + (rowHeight - trailing) * 0.5f));
        var pressed = IconButton(icon, $"{id}-action", tooltip: tooltip);
        ImGui.SetCursorScreenPos(after);
        return pressed;
    }

    private static bool DrawListRow(
        string id,
        string title,
        string detail,
        float trailing,
        out Vector2 origin,
        out float rowWidth,
        out float rowHeight)
    {
        var scale = MirageLayout.Style.Scale;
        var padX = 10f * scale;
        var padY = 8f * scale;
        var textGap = 2f * scale;
        var line = ImGui.GetTextLineHeight();
        var block = string.IsNullOrEmpty(detail) ? line : line + textGap + line;
        rowWidth = MathF.Max(1f, ImGui.GetContentRegionAvail().X);
        rowHeight = MathF.Max(block, trailing) + padY * 2f;

        using var rowScope = ImRaii.PushId(id);
        origin = ImGui.GetCursorScreenPos();
        ImGui.Dummy(new Vector2(rowWidth, rowHeight));

        var max = origin + new Vector2(rowWidth, rowHeight);
        var mouse = ImGui.GetMousePos();
        var trailingMinX = origin.X + rowWidth - padX - trailing;
        var onTrailing = mouse.X >= trailingMinX
            && mouse.X <= max.X
            && mouse.Y >= origin.Y
            && mouse.Y <= max.Y;
        var hovered = ImGui.IsWindowHovered() && ImGui.IsMouseHoveringRect(origin, max);
        var bodyHovered = hovered && !onTrailing;
        var held = bodyHovered && ImGui.IsMouseDown(ImGuiMouseButton.Left);

        if (hovered)
        {
            var fill = held ? ImGuiCol.HeaderActive : ImGuiCol.HeaderHovered;
            ImGui.GetWindowDrawList().AddRectFilled(origin, max, ImGui.GetColorU32(fill), InputStyle.FrameRounding * scale);
        }

        var textLeft = origin.X + padX;
        var textRight = trailingMinX - 8f * scale;
        var blockY = origin.Y + (rowHeight - block) * 0.5f;
        var dl = ImGui.GetWindowDrawList();
        dl.PushClipRect(new Vector2(textLeft, origin.Y), new Vector2(MathF.Max(textLeft + 1f, textRight), max.Y), true);
        dl.AddText(new Vector2(textLeft, blockY), ToUInt(GetColor(Color.Default)), title);
        if (!string.IsNullOrEmpty(detail))
            dl.AddText(new Vector2(textLeft, blockY + line + textGap), ToUInt(GetColor(Color.Secondary)), detail);
        dl.PopClipRect();

        return bodyHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
    }
}
