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

    /// <summary>
    /// Accent-styled button. Returns true when clicked.
    /// </summary>
    public static bool Button(
        string label,
        ButtonKind kind = ButtonKind.Primary,
        bool enabled = true,
        float width = 0f,
        string id = "")
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var (bg, hovered, active, border, text) = GetButtonColors(settings, kind);

        var framePadding = ResolveInputFramePadding();
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
            .Push(ImGuiStyleVar.FramePadding, new Vector2(
                framePadding.X + ButtonExtraPaddingX,
                framePadding.Y));

        if (!enabled)
            ImGui.BeginDisabled();

        var size = width > 0f ? new Vector2(width, 0f) : Vector2.Zero;
        var clicked = size == Vector2.Zero
            ? ImGui.Button(buttonId)
            : ImGui.Button(buttonId, size);

        if (!enabled)
            ImGui.EndDisabled();

        return clicked && enabled;
    }

    /// <summary>Primary accent button.</summary>
    public static bool PrimaryButton(string label, bool enabled = true, float width = 0f, string id = "") =>
        Button(label, ButtonKind.Primary, enabled, width, id);

    /// <summary>Secondary accent button.</summary>
    public static bool SecondaryButton(string label, bool enabled = true, float width = 0f, string id = "") =>
        Button(label, ButtonKind.Secondary, enabled, width, id);

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
