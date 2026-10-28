using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    public const float InputWidthFill = -1f;

    public static bool InputText(
        string label,
        ref string value,
        int maxLength = 256,
        string id = "",
        string? hint = null,
        float width = InputWidthFill,
        ImGuiInputTextFlags flags = ImGuiInputTextFlags.None,
        float labelWidth = FieldLabelColumnWidth)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, labelWidth: labelWidth, controlWidth: width))
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

    public static bool InputInt(
        string label,
        ref int value,
        int step = 1,
        int stepFast = 100,
        string id = "",
        float width = InputWidthFill,
        bool? liveMatch = null,
        string? matchTooltip = null,
        float labelWidth = FieldLabelColumnWidth)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, liveMatch, matchTooltip, labelWidth, width))
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

    public static bool InputFloat(
        string label,
        ref float value,
        float step = 0f,
        float stepFast = 0f,
        string format = "%.3f",
        string id = "",
        float width = InputWidthFill,
        float labelWidth = FieldLabelColumnWidth)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, labelWidth: labelWidth, controlWidth: width))
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

    public static bool DragFloat(
        string label,
        ref float value,
        float speed = 1f,
        float min = 0f,
        float max = 0f,
        string format = "%.1f",
        string id = "",
        float width = InputWidthFill,
        float labelWidth = FieldLabelColumnWidth)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, labelWidth: labelWidth, controlWidth: width))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return ImGui.DragFloat(widgetId, ref value, speed, min, max, format);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }

    public static bool DragInt(
        string label,
        ref int value,
        float speed = 1f,
        int min = 0,
        int max = 0,
        string id = "",
        float width = InputWidthFill,
        float labelWidth = FieldLabelColumnWidth)
    {
        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable, labelWidth: labelWidth, controlWidth: width))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return ImGui.DragInt(widgetId, ref value, speed, min, max);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }

    private static float ResolveFieldControlWidth(float width, bool inFieldTable)
    {
        if (inFieldTable)
            return -1f;

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
            .Push(ImGuiCol.Button, frame)
            .Push(ImGuiCol.ButtonHovered, hovered)
            .Push(ImGuiCol.ButtonActive, active)
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
