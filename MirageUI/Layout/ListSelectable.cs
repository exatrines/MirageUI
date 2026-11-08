namespace MirageUI.Layout;

using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;

internal static class ListSelectable
{
    internal static bool Draw(
        string label,
        bool selected,
        float rowHeight,
        float indent = 0f,
        float trailingWidth = 0f,
        bool dimmed = false,
        Vector4? labelColor = null,
        uint[]? gameIcons = null,
        FontAwesomeIcon icon = FontAwesomeIcon.Circle)
    {
        var g = ImGui.GetCurrentContext();
        var window = ImGuiP.GetCurrentWindow();
        var id = window.GetID("##Selectable"u8);
        var startScreenPos = MirageLayout.Cursor.ScreenPosition;
        var innerStartScreenPos = startScreenPos + MirageLayout.Style.ItemInnerSpacing.XOnly();
        innerStartScreenPos.X += indent;
        var endPos = startScreenPos + new Vector2(
            MirageLayout.Style.ContentRegionAvail.X - 12 - Math.Max(0f, trailingWidth),
            rowHeight);
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

        if (hovered || selected || dimmed)
        {
            var col = dimmed && !hovered && !selected
                ? ImGui.GetColorU32(ImGuiCol.Header, 0.35f)
                : ImGui.GetColorU32((held && hovered) ? ImGuiCol.HeaderActive : hovered ? ImGuiCol.HeaderHovered : ImGuiCol.Header);
            ImGuiP.RenderFrame(bb.Min, bb.Max, col, false, 3.0f);
        }

        ImGuiP.RenderNavHighlight(bb, id, ImGuiNavHighlightFlags.TypeThin | ImGuiNavHighlightFlags.NoRounding);

        var padding = MirageLayout.Style.FramePadding;
        var textMin = bb.Min + padding;
        var iconGap = MirageLayout.Style.ItemInnerSpacing.X;
        var cursorX = bb.Min.X + padding.X;
        var labelSize = ImGui.CalcTextSize(label);
        var alignY = MirageLayout.Style.SelectableTextAlign.Y;
        var textY = textMin.Y;
        if (alignY > 0f)
            textY += (bb.Max.Y - padding.Y - textY - labelSize.Y) * alignY;
        if (icon != FontAwesomeIcon.None && icon != FontAwesomeIcon.Circle)
        {
            var glyph = icon.ToIconString();
            ImGui.PushFont(UiBuilder.IconFont);
            var advance = ImGui.CalcTextSize(glyph);
            var origin = MirageIconButton.IconOrigin(
                new Vector2(cursorX, textY),
                new Vector2(advance.X, labelSize.Y),
                glyph);
            ImGui.GetWindowDrawList().AddText(origin, ImGui.GetColorU32(ImGuiCol.Text), glyph);
            ImGui.PopFont();
            cursorX += advance.X + iconGap;
        }

        if (gameIcons is { Length: > 0 })
        {
            var iconBox = MathF.Max(16f, rowHeight - padding.Y * 2f);
            var draw = ImGui.GetWindowDrawList();
            foreach (var iconId in gameIcons)
            {
                if (!TryDrawGameIcon(draw, iconId, new Vector2(cursorX, bb.Min.Y + (bb.GetHeight() - iconBox) * 0.5f), iconBox))
                    continue;

                cursorX += iconBox + iconGap;
            }
        }

        textMin.X = MathF.Max(textMin.X, cursorX);

        var pushedText = false;
        if (labelColor is { } tint)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, MirageUi.ToUInt(tint));
            pushedText = true;
        }
        else if (dimmed)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.Text, 0.45f));
            pushedText = true;
        }

        ImGuiP.RenderTextClipped(
            textMin,
            bb.Max - padding,
            label,
            Vector2.Zero,
            MirageLayout.Style.SelectableTextAlign,
            bb);
        if (pushedText)
            ImGui.PopStyleColor();

        return pressed;
    }

    internal readonly record struct FolderHeaderAction(bool Toggle, bool Select);

    internal static FolderHeaderAction DrawFolderHeader(
        string label,
        bool expanded,
        float rowHeight,
        bool canCollapse = true,
        float trailingWidth = 0f,
        bool selected = false,
        bool selectable = false,
        uint gameIconId = 0)
    {
        var window = ImGuiP.GetCurrentWindow();
        var id = window.GetID("##Folder"u8);
        var startScreenPos = MirageLayout.Cursor.ScreenPosition;
        var innerStartScreenPos = startScreenPos + MirageLayout.Style.ItemInnerSpacing.XOnly();
        var endPos = startScreenPos + new Vector2(
            MirageLayout.Style.ContentRegionAvail.X - 12 - Math.Max(0f, trailingWidth),
            rowHeight);
        var bb = new ImRect(innerStartScreenPos, endPos);
        ImGuiP.ItemSize(new Vector2(bb.GetWidth(), rowHeight));
        if (!ImGuiP.ItemAdd(bb, id))
            return default;

        var padding = MirageLayout.Style.FramePadding;
        var iconGap = MirageLayout.Style.ItemInnerSpacing.X;
        IDalamudTextureWrap? gameTexture = null;
        var gameIcon = gameIconId != 0 && MirageUi.TryGetGameIcon(gameIconId, out gameTexture);
        var iconBox = MathF.Max(16f, rowHeight - padding.Y * 2f);
        string? iconText = null;
        Vector2 iconSize;
        if (gameIcon)
            iconSize = new Vector2(iconBox, iconBox);
        else
        {
            var icon = expanded ? FontAwesomeIcon.FolderOpen : FontAwesomeIcon.Folder;
            ImGui.PushFont(UiBuilder.IconFont);
            iconText = icon.ToIconString();
            iconSize = ImGui.CalcTextSize(iconText);
            ImGui.PopFont();
        }

        var iconEnd = bb.Min.X + padding.X + iconSize.X + iconGap;

        var hovered = false;
        var held = false;
        var pressed = false;
        if (canCollapse || selectable)
        {
            pressed = ImGuiP.ButtonBehavior(bb, id, ref hovered, ref held, ImGuiButtonFlags.None);
            if (pressed)
                ImGuiP.MarkItemEdited(id);

            if (hovered || selected)
            {
                var col = ImGui.GetColorU32(
                    held && hovered ? ImGuiCol.HeaderActive
                    : hovered ? ImGuiCol.HeaderHovered
                    : ImGuiCol.Header);
                ImGuiP.RenderFrame(bb.Min, bb.Max, col, false, 3.0f);
            }
        }

        var toggle = false;
        var select = false;
        if (pressed)
        {
            var onIcon = canCollapse && ImGui.GetMousePos().X < iconEnd;
            if (selectable && !onIcon)
            {
                select = true;
                if (canCollapse && !expanded)
                    toggle = true;
            }
            else if (canCollapse)
            {
                toggle = true;
            }
        }

        var labelSize = ImGui.CalcTextSize(label);
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var dl = ImGui.GetWindowDrawList();
        var iconPos = new Vector2(
            bb.Min.X + padding.X,
            bb.Min.Y + (bb.GetHeight() - iconSize.Y) * 0.5f);
        var labelPos = new Vector2(
            iconPos.X + iconSize.X + iconGap,
            bb.Min.Y + (bb.GetHeight() - labelSize.Y) * 0.5f);

        if (gameIcon)
            dl.AddImage(gameTexture!.Handle, iconPos, iconPos + iconSize);
        else
        {
            ImGui.PushFont(UiBuilder.IconFont);
            dl.AddText(iconPos, textColor, iconText!);
            ImGui.PopFont();
        }

        dl.AddText(labelPos, textColor, label);
        return new FolderHeaderAction(toggle, select);
    }

    private static bool TryDrawGameIcon(ImDrawListPtr draw, uint iconId, Vector2 pos, float size)
    {
        if (iconId == 0 || !MirageUi.TryGetGameIcon(iconId, out var texture))
            return false;

        draw.AddImage(texture.Handle, pos, pos + new Vector2(size, size));
        return true;
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
