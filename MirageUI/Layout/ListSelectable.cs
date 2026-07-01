namespace MirageUI.Layout;

internal static class ListSelectable
{
    internal static bool Draw(string label, bool selected, float rowHeight)
    {
        var g = ImGui.GetCurrentContext();
        var window = ImGuiP.GetCurrentWindow();
        var id = window.GetID("##Selectable"u8);
        var startScreenPos = MirageLayout.Cursor.ScreenPosition;
        var innerStartScreenPos = startScreenPos + MirageLayout.Style.ItemInnerSpacing.XOnly();
        var endPos = startScreenPos + new Vector2(MirageLayout.Style.ContentRegionAvail.X - 12, rowHeight);
        var bb = new ImRect(innerStartScreenPos, endPos);
        ImGuiP.ItemSize(new Vector2(bb.GetWidth(), rowHeight));
        if (!ImGuiP.ItemAdd(bb, id))
            return false;

        var hovered = false;
        var held = false;
        var pressed = ImGuiP.ButtonBehavior(bb, id, ref hovered, ref held, ImGuiButtonFlags.None);
        if (g.NavJustMovedToId != 0 && g.NavJustMovedToFocusScopeId == window.DC.NavFocusScopeIdCurrent && g.NavJustMovedToId == id)
            selected = pressed = true;

        if (pressed && !g.NavDisableMouseHover && g.NavWindow == window && g.NavLayer == window.DC.NavLayerCurrent)
        {
            ImGuiP.SetNavID(id, window.DC.NavLayerCurrent, window.DC.NavFocusScopeIdCurrent, ImGuiP.WindowRectAbsToRel(window, bb));
            g.NavDisableHighlight = true;
        }

        if (pressed)
            ImGuiP.MarkItemEdited(id);

        if (hovered || selected)
        {
            var col = ImGui.GetColorU32((held && hovered) ? ImGuiCol.HeaderActive : hovered ? ImGuiCol.HeaderHovered : ImGuiCol.Header);
            ImGuiP.RenderFrame(bb.Min, bb.Max, col, false, 3.0f);
        }

        ImGuiP.RenderNavHighlight(bb, id, ImGuiNavHighlightFlags.TypeThin | ImGuiNavHighlightFlags.NoRounding);

        var padding = MirageLayout.Style.FramePadding;
        ImGuiP.RenderTextClipped(
            bb.Min + padding,
            bb.Max - padding,
            label,
            Vector2.Zero,
            MirageLayout.Style.SelectableTextAlign,
            bb);
        return pressed;
    }
}
