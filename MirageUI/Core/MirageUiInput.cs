using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>Fill remaining content width (ImGui <c>-1</c>).</summary>
    public const float InputWidthFill = -1f;

    /// <summary>
    /// Accent-themed text field. Layout: [label | input] via Table API.
    /// Right column fills remaining width. Empty <paramref name="label"/> = control only.
    /// </summary>
    public static bool InputText(
        string label,
        ref string value,
        int maxLength = 256,
        string id = "",
        string? hint = null,
        float width = InputWidthFill,
        ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return hint == null
                ? ImGui.InputText(widgetId, ref value, maxLength, flags)
                : ImGui.InputTextWithHint(widgetId, hint, ref value, maxLength, flags);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }

    /// <summary>
    /// Accent-themed integer field. Layout: [label | input] via Table API.
    /// </summary>
    public static bool InputInt(
        string label,
        ref int value,
        int step = 1,
        int stepFast = 100,
        string id = "",
        float width = InputWidthFill,
        bool? liveMatch = null,
        string? matchTooltip = null)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, liveMatch, matchTooltip))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return ImGui.InputInt(widgetId, ref value, step, stepFast);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }

    /// <summary>
    /// Accent-themed float field. Layout: [label | input] via Table API.
    /// </summary>
    public static bool InputFloat(
        string label,
        ref float value,
        float step = 0f,
        float stepFast = 0f,
        string format = "%.3f",
        string id = "",
        float width = InputWidthFill)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return ImGui.InputFloat(widgetId, ref value, step, stepFast, format);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }

    /// <summary>
    /// In a table field column, default to fill. Outside a table: fixed / half / fill as before.
    /// </summary>
    private static float ResolveFieldControlWidth(float width, bool inFieldTable)
    {
        if (inFieldTable)
            return width > 0f ? width : -1f;

        if (width > 0f)
            return width;

        var avail = MirageLayout.Style.ContentRegionAvail.X;
        if (width == 0f)
            return MathF.Max(1f, avail * 0.5f);

        return -1f;
    }

    private static readonly Stack<(ImRaii.ColorDisposable Colors, ImRaii.StyleDisposable Styles)> InputScopes = new();

    private static void PushInputChrome()
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var accent = settings.GetColor(Color.Accent);
        var (frame, hovered, active, border, _, _, _, _) = GetDropdownColors(settings, accent);
        var framePadding = ResolveInputFramePadding();

        var colors = new ImRaii.ColorDisposable()
            .Push(ImGuiCol.FrameBg, frame)
            .Push(ImGuiCol.FrameBgHovered, hovered)
            .Push(ImGuiCol.FrameBgActive, active)
            .Push(ImGuiCol.Border, border)
            .Push(ImGuiCol.SliderGrab, WithAlpha(accent, 0.75f))
            .Push(ImGuiCol.SliderGrabActive, accent)
            .Push(ImGuiCol.Text, settings.GetColor(Color.Default))
            .Push(ImGuiCol.TextDisabled, settings.GetColor(Color.Secondary));

        var styles = new ImRaii.StyleDisposable()
            .Push(ImGuiStyleVar.FrameRounding, InputStyle.FrameRounding)
            .Push(ImGuiStyleVar.FrameBorderSize, 1f)
            .Push(ImGuiStyleVar.FramePadding, framePadding);

        InputScopes.Push((colors, styles));
    }

    private static void PopInputChrome()
    {
        if (InputScopes.Count == 0)
            return;

        var (colors, styles) = InputScopes.Pop();
        styles.Dispose();
        colors.Dispose();
    }
}
