namespace MirageUI.Layout;

using Dalamud.Interface;

internal static class ListSelectable
{
    internal static bool Draw(string label, bool selected, float rowHeight, float indent = 0f)
    {
        var g = ImGui.GetCurrentContext();
        var window = ImGuiP.GetCurrentWindow();
        var id = window.GetID("##Selectable"u8);
        var startScreenPos = MirageLayout.Cursor.ScreenPosition;
        var innerStartScreenPos = startScreenPos + MirageLayout.Style.ItemInnerSpacing.XOnly();
        innerStartScreenPos.X += indent;
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

    internal static bool DrawFolderHeader(string label, bool expanded, float rowHeight)
    {
        var window = ImGuiP.GetCurrentWindow();
        var id = window.GetID("##Folder"u8);
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
        if (pressed)
            ImGuiP.MarkItemEdited(id);

        if (hovered)
        {
            var col = ImGui.GetColorU32((held && hovered) ? ImGuiCol.HeaderActive : ImGuiCol.HeaderHovered);
            ImGuiP.RenderFrame(bb.Min, bb.Max, col, false, 3.0f);
        }

        var padding = MirageLayout.Style.FramePadding;
        var iconGap = MirageLayout.Style.ItemInnerSpacing.X;
        var icon = expanded ? FontAwesomeIcon.FolderOpen : FontAwesomeIcon.Folder;

        ImGui.PushFont(UiBuilder.IconFont);
        var iconText = icon.ToIconString();
        var iconSize = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();

        var labelSize = ImGui.CalcTextSize(label);
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var dl = ImGui.GetWindowDrawList();
        var iconPos = new Vector2(
            bb.Min.X + padding.X,
            bb.Min.Y + (bb.GetHeight() - iconSize.Y) * 0.5f);
        var labelPos = new Vector2(
            iconPos.X + iconSize.X + iconGap,
            bb.Min.Y + (bb.GetHeight() - labelSize.Y) * 0.5f);

        ImGui.PushFont(UiBuilder.IconFont);
        dl.AddText(iconPos, textColor, iconText);
        ImGui.PopFont();

        dl.AddText(labelPos, textColor, label);
        return pressed;
    }

    internal static bool DrawRunButton(ref bool isRunning, MirageTwoColumnRunBinding run, float size)
    {
        var icon = isRunning ? FontAwesomeIcon.Stop : FontAwesomeIcon.Play;
        var color = isRunning
            ? MirageUi.GetColor(MirageUi.Color.Accent)
            : MirageUi.GetColor(MirageUi.Color.Default);
        var hoverColor = isRunning ? MirageIconButton.RunRed : MirageIconButton.RunGreen;
        var tooltip = isRunning ? run.StopTooltip : run.RunTooltip;
        var clicked = MirageUi.IconButton(
            icon,
            "##Run",
            new Vector2(size, size),
            color,
            hoverColor,
            hoveredBackgroundColor: MirageIconButton.DefaultBackground,
            activeBackgroundColor: MirageIconButton.DefaultBackground,
            tooltip: tooltip,
            tooltipPosition: run.TooltipPosition);
        if (!clicked)
            return false;

        if (isRunning)
            isRunning = false;
        else
        {
            isRunning = true;
            run.OnRun?.Invoke();
        }

        return true;
    }
}
