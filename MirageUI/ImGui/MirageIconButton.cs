using Dalamud.Interface;
using MirageUI.Theme;

namespace MirageUI.Ui;

internal static class MirageIconButton
{
    internal static readonly Vector4 DefaultBackground = new(1f, 1f, 1f, 0f);
    internal static readonly Vector4 DefaultHoveredBackground = new(1f, 1f, 1f, 0.08f);
    internal static readonly Vector4 DefaultActiveBackground = new(1f, 1f, 1f, 0.16f);

    internal static readonly Vector4 RunGreen = new(0.45f, 0.85f, 0.45f, 1f);
    internal static readonly Vector4 RunRed = new(0.9f, 0.35f, 0.35f, 1f);

    private const float FrameRounding = 3f;

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
        bool enabled,
        bool border)
    {
        if (size == Vector2.Zero)
        {
            var height = MirageUi.ResolveControlHeight();
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
        {
            // Transparent icons: hover via text color only (no fill flash).
            bg = hoveredBackgroundColor
                 ?? (backgroundColor.W <= 0f ? backgroundColor : ResolveHoveredBackground(backgroundColor));
        }

        if (border || bg.W > 0f)
        {
            if (border)
            {
                var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
                var borderColor = settings.GetColor(MirageUi.Color.Accent);
                using (ImRaii.PushColor(ImGuiCol.Border, borderColor))
                using (ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1f))
                    ImGuiP.RenderFrame(bb.Min, bb.Max, MirageUi.ToUInt(bg), true, FrameRounding);
            }
            else
            {
                ImGuiP.RenderFrame(bb.Min, bb.Max, MirageUi.ToUInt(bg), false, 0f);
            }
        }

        ImGuiP.RenderNavHighlight(bb, widgetId, ImGuiNavHighlightFlags.TypeThin | ImGuiNavHighlightFlags.NoRounding);

        var displayIcon = enabled && hovered && hoverIcon is { } hover ? hover : icon;
        var label = displayIcon.ToIconString();

        var settingsForText = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var drawColor = enabled ? textColor : WithAlpha(textColor, 0.6f);
        if (enabled && hovered)
            drawColor = hoverTextColor ?? settingsForText.GetColor(MirageUi.Color.Accent);

        ImGui.PushFont(UiBuilder.IconFont);
        var iconPos = IconOrigin(bb.Min, size, label);
        ImGui.GetWindowDrawList().AddText(iconPos, MirageUi.ToUInt(drawColor), label);
        ImGui.PopFont();

        if (hovered && !string.IsNullOrEmpty(tooltip))
            MirageTooltip.Show(tooltip, tooltipPosition, bb);

        return pressed;
    }

    // Center the painted glyph, not the font advance box. Advance centering leaves the ink left of the frame.
    private static unsafe Vector2 IconOrigin(Vector2 boxMin, Vector2 boxSize, string label)
    {
        var font = ImGui.GetFont();
        var glyph = label.Length > 0 ? font.FindGlyph(label[0]) : null;
        if (glyph == null)
            return boxMin + (boxSize - ImGui.CalcTextSize(label)) * 0.5f;

        var scale = ImGui.GetFontSize() / font.FontSize;
        var inkCenter = new Vector2(glyph->X0 + glyph->X1, glyph->Y0 + glyph->Y1) * (0.5f * scale);
        var origin = boxMin + boxSize * 0.5f - inkCenter;
        return new Vector2(
            MathF.Round(origin.X, MidpointRounding.AwayFromZero),
            MathF.Round(origin.Y, MidpointRounding.AwayFromZero));
    }

    private static Vector4 ResolveHoveredBackground(Vector4 backgroundColor) =>
        backgroundColor.W <= 0f ? DefaultHoveredBackground : WithAlpha(backgroundColor, 1.25f);

    private static Vector4 ResolveActiveBackground(Vector4 backgroundColor) =>
        backgroundColor.W <= 0f ? DefaultActiveBackground : WithAlpha(backgroundColor, 1.5f);

    private static Vector4 WithAlpha(Vector4 color, float alphaMultiplier) =>
        new(color.X, color.Y, color.Z, Math.Clamp(color.W * alphaMultiplier, 0f, 1f));
}
