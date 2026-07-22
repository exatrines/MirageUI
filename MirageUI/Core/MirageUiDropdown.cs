using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    private const float DropdownFrameRounding = 4f;
    private const float DropdownPopupRounding = 4f;

    /// <summary>
    /// Default width: half of the right-column content width.
    /// </summary>
    public const float DropdownWidthHalfMain = 0f;

    private static readonly Stack<(ImRaii.ColorDisposable Colors, ImRaii.StyleDisposable Styles, string Label)> DropdownScopes = new();

    /// <summary>
    /// Accent-colored dropdown. Returns true when the selection changes.
    /// Layout: [combo] [label]. Default width is half the right-column content.
    /// </summary>
    public static bool Dropdown(
        string label,
        ref string selected,
        IReadOnlyList<string> items,
        string? placeholder = "(not set)",
        string id = "",
        bool allowClear = true,
        string? emptyMessage = null,
        float width = DropdownWidthHalfMain)
    {
        var preview = string.IsNullOrWhiteSpace(selected)
            ? (placeholder ?? string.Empty)
            : selected;

        if (!BeginDropdown(label, preview, id, width, hasValue: !string.IsNullOrWhiteSpace(selected)))
            return false;

        var changed = false;

        if (allowClear && placeholder != null)
        {
            if (DropdownItem(placeholder, string.IsNullOrWhiteSpace(selected)))
            {
                if (!string.IsNullOrWhiteSpace(selected))
                {
                    selected = string.Empty;
                    changed = true;
                }
            }
        }

        if (items.Count == 0)
        {
            if (!string.IsNullOrEmpty(emptyMessage))
                ImGui.TextDisabled(emptyMessage);
        }
        else
        {
            foreach (var item in items)
            {
                var isSelected = string.Equals(selected, item, StringComparison.Ordinal);
                if (!DropdownItem(item, isSelected))
                    continue;

                if (isSelected)
                    continue;

                selected = item;
                changed = true;
            }
        }

        EndDropdown();
        return changed;
    }

    /// <summary>
    /// Starts an accent-colored dropdown. When true, draw items then call <see cref="EndDropdown"/>.
    /// Layout: [combo] [label]. Default width is half the right-column content.
    /// </summary>
    public static bool BeginDropdown(
        string label,
        string preview,
        string id = "",
        float width = DropdownWidthHalfMain,
        bool hasValue = true)
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var accent = settings.GetColor(Color.Accent);
        var (frame, hovered, active, border, header, headerHovered, headerActive, popupBg) =
            GetDropdownColors(settings, accent);

        var colors = new ImRaii.ColorDisposable()
            .Push(ImGuiCol.FrameBg, frame)
            .Push(ImGuiCol.FrameBgHovered, hovered)
            .Push(ImGuiCol.FrameBgActive, active)
            .Push(ImGuiCol.Border, border)
            .Push(ImGuiCol.Header, header)
            .Push(ImGuiCol.HeaderHovered, headerHovered)
            .Push(ImGuiCol.HeaderActive, headerActive)
            .Push(ImGuiCol.PopupBg, popupBg)
            .Push(ImGuiCol.Button, frame)
            .Push(ImGuiCol.ButtonHovered, hovered)
            .Push(ImGuiCol.ButtonActive, active)
            .Push(ImGuiCol.CheckMark, accent);

        var styles = new ImRaii.StyleDisposable()
            .Push(ImGuiStyleVar.FrameRounding, DropdownFrameRounding)
            .Push(ImGuiStyleVar.PopupRounding, DropdownPopupRounding)
            .Push(ImGuiStyleVar.FrameBorderSize, 1f)
            .Push(ImGuiStyleVar.PopupBorderSize, 1f);

        DropdownScopes.Push((colors, styles, label));

        var comboId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";

        var previewColor = hasValue
            ? settings.GetColor(Color.Default)
            : settings.GetColor(Color.Secondary);
        ImGui.PushStyleColor(ImGuiCol.Text, previewColor);
        ImGui.SetNextItemWidth(ResolveDropdownWidth(width));
        var open = ImGui.BeginCombo(comboId, preview, ImGuiComboFlags.None);
        ImGui.PopStyleColor();

        if (!open)
        {
            // Closed: still in the parent window — draw trailing label here.
            DrawTrailingLabel(label);
            PopDropdownScope();
            return false;
        }

        // Open: do not draw the label inside the popup (BeginCombo redirects widgets there).
        return true;
    }

    /// <summary>Call when <see cref="BeginDropdown"/> returns true.</summary>
    public static void EndDropdown()
    {
        if (DropdownScopes.Count == 0)
        {
            ImGui.EndCombo();
            return;
        }

        var label = DropdownScopes.Peek().Label;
        ImGui.EndCombo();
        // After EndCombo we are back in the parent window — draw trailing label beside the combo.
        DrawTrailingLabel(label);
        PopDropdownScope();
    }

    /// <summary>Dropdown item. Focuses the selected row when open.</summary>
    public static bool DropdownItem(string label, bool selected)
    {
        var clicked = ImGui.Selectable(label, selected);
        if (selected)
            ImGui.SetItemDefaultFocus();
        return clicked;
    }

    /// <summary>
    /// Resolves width. Values &lt;= 0 use half of the available content width.
    /// </summary>
    private static float ResolveDropdownWidth(float width) =>
        width > 0f
            ? width
            : MathF.Max(1f, MirageLayout.Style.ContentRegionAvail.X * 0.5f);

    private static void DrawTrailingLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
            return;

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);
    }

    private static void PopDropdownScope()
    {
        if (DropdownScopes.Count == 0)
            return;

        var (colors, styles, _) = DropdownScopes.Pop();
        styles.Dispose();
        colors.Dispose();
    }

    private static (
        Vector4 Frame,
        Vector4 Hovered,
        Vector4 Active,
        Vector4 Border,
        Vector4 Header,
        Vector4 HeaderHovered,
        Vector4 HeaderActive,
        Vector4 PopupBg)
        GetDropdownColors(MirageColorSettings settings, Vector4 accent)
    {
        var frame = WithAlpha(accent, 0.14f);
        var hovered = WithAlpha(accent, 0.26f);
        var active = WithAlpha(accent, 0.38f);
        var border = WithAlpha(accent, 0.75f);
        var header = WithAlpha(accent, 0.32f);
        var headerHovered = WithAlpha(accent, 0.48f);
        var headerActive = WithAlpha(accent, 0.60f);
        var popupBg = Mix(settings.WindowBg, accent, 0.08f);
        return (frame, hovered, active, border, header, headerHovered, headerActive, popupBg);
    }

    private static Vector4 Mix(Vector4 a, Vector4 b, float t) =>
        new(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t,
            a.W + (b.W - a.W) * t);
}
