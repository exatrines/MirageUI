using Dalamud.Interface;

namespace MirageUI.Ui;

internal static class MirageIconButton
{
    internal static readonly Vector4 DefaultBackground = new(1f, 1f, 1f, 0f);
    internal static readonly Vector4 DefaultText = new(1f, 1f, 1f, 1f);
    internal static readonly Vector4 DefaultHoveredBackground = new(1f, 1f, 1f, 0.08f);
    internal static readonly Vector4 DefaultActiveBackground = new(1f, 1f, 1f, 0.16f);

    internal static readonly Vector4 RunGreen = new(0.45f, 0.85f, 0.45f, 1f);
    internal static readonly Vector4 RunRed = new(0.9f, 0.35f, 0.35f, 1f);

    private const float PaddingTop = -1f;
    private const float PaddingBottom = 1f;

    internal static bool Draw(
        FontAwesomeIcon icon,
        FontAwesomeIcon? hoverIcon,
        string id,
        Vector2 size,
        Vector4 textColor,
        Vector4? hoverTextColor,
        Vector4 backgroundColor,
        Vector4? hoveredBackgroundColor,
        Vector4? activeBackgroundColor,
        string? tooltip,
        MirageTooltipPosition tooltipPosition,
        bool enabled)
    {
        if (size == Vector2.Zero)
        {
            var height = MirageLayout.Style.FrameHeight;
            size = new Vector2(height, height);
        }

        var window = ImGuiP.GetCurrentWindow();
        var widgetId = string.IsNullOrEmpty(id) ? window.GetID("##MirageIconButton"u8) : window.GetID(id);

        var pos = MirageLayout.Cursor.ScreenPosition;
        var bb = new ImRect(pos, pos + size);
        ImGuiP.ItemSize(size);
        if (!ImGuiP.ItemAdd(bb, widgetId))
            return false;

        var hovered = false;
        var held = false;
        var pressed = ImGuiP.ButtonBehavior(bb, widgetId, ref hovered, ref held) && enabled;
        if (pressed)
            ImGuiP.MarkItemEdited(widgetId);

        var bg = backgroundColor;
        if (enabled && held && hovered)
            bg = activeBackgroundColor ?? ResolveActiveBackground(backgroundColor);
        else if (enabled && hovered)
            bg = hoveredBackgroundColor ?? ResolveHoveredBackground(backgroundColor);

        if (bg.W > 0f)
            ImGuiP.RenderFrame(bb.Min, bb.Max, MirageUi.ToUInt(bg), false, 0f);

        ImGuiP.RenderNavHighlight(bb, widgetId, ImGuiNavHighlightFlags.TypeThin | ImGuiNavHighlightFlags.NoRounding);

        var displayIcon = enabled && hovered && hoverIcon is { } hover ? hover : icon;
        var label = displayIcon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(label);
        ImGui.PopFont();

        var padX = MirageLayout.Style.FramePadding.X;
        var availMin = new Vector2(bb.Min.X + padX, bb.Min.Y + PaddingTop);
        var availMax = new Vector2(bb.Max.X - padX, bb.Max.Y - PaddingBottom);
        var iconPos = new Vector2(
            availMin.X + (availMax.X - availMin.X - iconSize.X) * 0.5f,
            availMin.Y + (availMax.Y - availMin.Y - iconSize.Y) * 0.5f);

        var drawColor = enabled ? textColor : WithAlpha(textColor, 0.6f);
        if (enabled && hovered)
            drawColor = hoverTextColor ?? drawColor;

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.GetWindowDrawList().AddText(iconPos, MirageUi.ToUInt(drawColor), label);
        ImGui.PopFont();

        if (enabled && hovered && !string.IsNullOrEmpty(tooltip))
            MirageTooltip.Show(tooltip, tooltipPosition, bb);

        return pressed;
    }

    private static Vector4 ResolveHoveredBackground(Vector4 backgroundColor) =>
        backgroundColor.W <= 0f ? DefaultHoveredBackground : WithAlpha(backgroundColor, 1.25f);

    private static Vector4 ResolveActiveBackground(Vector4 backgroundColor) =>
        backgroundColor.W <= 0f ? DefaultActiveBackground : WithAlpha(backgroundColor, 1.5f);

    private static Vector4 WithAlpha(Vector4 color, float alphaMultiplier) =>
        new(color.X, color.Y, color.Z, Math.Clamp(color.W * alphaMultiplier, 0f, 1f));
}
