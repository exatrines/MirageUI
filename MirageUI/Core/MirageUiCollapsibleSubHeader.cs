using Dalamud.Interface;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// Chevron + title sub-header. Hover uses accent wash.
    /// Returns the current open state (same as ImGui collapsing headers).
    /// </summary>
    public static bool CollapsibleSubHeader(
        string label,
        ref bool open,
        string id = "")
    {
        var scale = MirageLayout.Style.Scale;
        var rowH = ResolveControlHeight();
        var avail = ImGui.GetContentRegionAvail().X;
        var accent = GetColor(Color.Accent);
        var title = GetColor(Color.Title);
        var buttonId = string.IsNullOrEmpty(id) ? "##collapsibleSubHeader" : id;

        MirageLayout.Cursor.Y += MirageLayout.Style.ItemSpacing.Y;

        var min = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton(buttonId, new Vector2(MathF.Max(1f, avail), rowH));
        var hovered = ImGui.IsItemHovered();
        var held = hovered && ImGui.IsMouseDown(ImGuiMouseButton.Left);
        if (ImGui.IsItemClicked())
            open = !open;

        var max = min + new Vector2(avail, rowH);
        var dl = ImGui.GetWindowDrawList();
        if (hovered)
        {
            var fill = held ? WithAlpha(accent, 0.38f) : WithAlpha(accent, 0.22f);
            dl.AddRectFilled(min, max, ToUInt(fill), 3f * scale);
        }

        var icon = open ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronRight;
        ImGui.PushFont(UiBuilder.IconFont);
        var iconText = icon.ToIconString();
        var iconSize = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();
        var labelSize = ImGui.CalcTextSize(label);
        var gap = MirageLayout.Style.ItemInnerSpacing.X;
        var pad = MirageLayout.Style.FramePadding.X;
        var iconPos = new Vector2(
            min.X + pad,
            min.Y + (rowH - iconSize.Y) * 0.5f);
        var labelPos = new Vector2(
            iconPos.X + iconSize.X + gap,
            min.Y + (rowH - labelSize.Y) * 0.5f);

        ImGui.PushFont(UiBuilder.IconFont);
        dl.AddText(iconPos, ToUInt(accent), iconText);
        ImGui.PopFont();
        dl.AddText(labelPos, ToUInt(title), label);

        return open;
    }
}
