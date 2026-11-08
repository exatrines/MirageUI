using Dalamud.Interface;
using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    public enum ButtonKind
    {
        Primary,
        Secondary,
    }

    private const float ButtonFrameRounding = 3f;
    private const float ButtonExtraPaddingX = 6f;

    public static bool Button(
        string label,
        ButtonKind kind = ButtonKind.Primary,
        bool enabled = true,
        float width = 0f,
        string id = "",
        float height = 0f)
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var (bg, hovered, active, border, text) = GetButtonColors(settings, kind);

        var framePadding = ResolveInputFramePadding();
        var padY = framePadding.Y;
        if (height > 0f)
            padY = MathF.Max(0f, (height - ImGui.GetTextLineHeight()) * 0.5f);
        var buttonId = string.IsNullOrEmpty(id) ? label : $"{label}##{id}";

        using var colors = ImRaii
            .PushColor(ImGuiCol.Button, bg)
            .Push(ImGuiCol.ButtonHovered, hovered)
            .Push(ImGuiCol.ButtonActive, active)
            .Push(ImGuiCol.Text, text)
            .Push(ImGuiCol.Border, border);
        using var styles = ImRaii
            .PushStyle(ImGuiStyleVar.FrameRounding, ButtonFrameRounding)
            .Push(ImGuiStyleVar.FrameBorderSize, 1f)
            .Push(ImGuiStyleVar.FramePadding, new Vector2(framePadding.X + ButtonExtraPaddingX, padY));

        if (!enabled)
            ImGui.BeginDisabled();

        var size = width > 0f || height > 0f ? new Vector2(width, height) : Vector2.Zero;
        var clicked = size == Vector2.Zero
            ? ImGui.Button(buttonId)
            : ImGui.Button(buttonId, size);

        if (!enabled)
            ImGui.EndDisabled();

        return clicked && enabled;
    }

    public static bool PrimaryButton(string label, bool enabled = true, float width = 0f, string id = "", float height = 0f) =>
        Button(label, ButtonKind.Primary, enabled, width, id, height);

    public static float ButtonWidth(string label)
    {
        var padX = ResolveInputFramePadding().X + ButtonExtraPaddingX;
        return ImGui.CalcTextSize(label).X + padX * 2f;
    }

    public static bool SecondaryButton(string label, bool enabled = true, float width = 0f, string id = "") =>
        Button(label, ButtonKind.Secondary, enabled, width, id);

    public static bool IconLabelButton(
        FontAwesomeIcon icon,
        string label,
        ButtonKind kind = ButtonKind.Primary,
        bool enabled = true,
        float width = 0f,
        string id = "",
        string? tooltip = null,
        MirageTooltipPosition tooltipPosition = MirageTooltipPosition.Above)
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var text = settings.GetColor(kind == ButtonKind.Primary ? Color.Default : Color.Secondary);
        var buttonId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
        var gap = ImGui.GetStyle().ItemInnerSpacing.X;
        var iconText = icon.ToIconString();
        Vector2 iconSize;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            iconSize = ImGui.CalcTextSize(iconText);
        var labelSize = ImGui.CalcTextSize(label);
        var buttonWidth = width > 0f ? width : iconSize.X + gap + labelSize.X;
        var clicked = ImGui.InvisibleButton(buttonId, new Vector2(buttonWidth, ResolveControlHeight()));
        var hovered = enabled && ImGui.IsItemHovered();
        var color = !enabled
            ? WithAlpha(text, 0.45f)
            : hovered
                ? settings.GetColor(Color.Accent)
                : text;
        DrawIconLabel(iconText, iconSize, label, labelSize, gap, color);
        if (!string.IsNullOrEmpty(tooltip))
            Tooltip(tooltip, tooltipPosition);
        return clicked && enabled;
    }

    public static bool PrimaryIconLabelButton(
        FontAwesomeIcon icon,
        string label,
        bool enabled = true,
        float width = 0f,
        string id = "",
        string? tooltip = null,
        MirageTooltipPosition tooltipPosition = MirageTooltipPosition.Above) =>
        IconLabelButton(icon, label, ButtonKind.Primary, enabled, width, id, tooltip, tooltipPosition);

    public static bool SecondaryIconLabelButton(
        FontAwesomeIcon icon,
        string label,
        bool enabled = true,
        float width = 0f,
        string id = "",
        string? tooltip = null,
        MirageTooltipPosition tooltipPosition = MirageTooltipPosition.Above) =>
        IconLabelButton(icon, label, ButtonKind.Secondary, enabled, width, id, tooltip, tooltipPosition);

    public static float IconLabelButtonWidth(FontAwesomeIcon icon, string label)
    {
        var gap = ImGui.GetStyle().ItemInnerSpacing.X;
        Vector2 iconSize;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            iconSize = ImGui.CalcTextSize(icon.ToIconString());
        return iconSize.X + gap + ImGui.CalcTextSize(label).X;
    }

    private static void DrawIconLabel(
        string iconText,
        Vector2 iconSize,
        string label,
        Vector2 labelSize,
        float gap,
        Vector4 color)
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var packed = ToUInt(color);
        var draw = ImGui.GetWindowDrawList();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var iconY = min.Y + (max.Y - min.Y - iconSize.Y) * 0.5f;
            draw.AddText(new Vector2(min.X, MathF.Round(iconY)), packed, iconText);
        }

        var textY = min.Y + (max.Y - min.Y - labelSize.Y) * 0.5f;
        draw.AddText(new Vector2(min.X + iconSize.X + gap, MathF.Round(textY)), packed, label);
    }

    private static (Vector4 Bg, Vector4 Hovered, Vector4 Active, Vector4 Border, Vector4 Text)
        GetButtonColors(MirageColorSettings settings, ButtonKind kind)
    {
        var accent = settings.GetColor(Color.Accent);
        if (kind == ButtonKind.Primary)
        {
            return (
                WithAlpha(accent, 0.30f),
                WithAlpha(accent, 0.46f),
                WithAlpha(accent, 0.60f),
                accent,
                settings.GetColor(Color.Default));
        }

        return (
            WithAlpha(accent, 0.10f),
            WithAlpha(accent, 0.22f),
            WithAlpha(accent, 0.34f),
            WithAlpha(accent, 0.50f),
            settings.GetColor(Color.Secondary));
    }
}
