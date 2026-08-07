namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// Accent-themed float slider. Layout: [label | slider] via Table API.
    /// Empty <paramref name="label"/> = control only.
    /// </summary>
    public static bool SliderFloat(
        string label,
        ref float value,
        float min,
        float max,
        string format = "%.1f",
        string id = "",
        float width = InputWidthFill)
    {
        value = Math.Clamp(value, min, max);

        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return ImGui.SliderFloat(widgetId, ref value, min, max, format);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }

    /// <summary>
    /// Accent-themed integer slider. Layout: [label | slider] via Table API.
    /// Empty <paramref name="label"/> = control only.
    /// </summary>
    public static bool SliderInt(
        string label,
        ref int value,
        int min,
        int max,
        string id = "",
        float width = InputWidthFill)
    {
        value = Math.Clamp(value, min, max);

        var fieldId = string.IsNullOrEmpty(id) ? label : id;
        if (!BeginFieldRow(label, fieldId, out var usedTable))
            return false;

        PushInputChrome();
        try
        {
            ImGui.SetNextItemWidth(ResolveFieldControlWidth(width, usedTable));
            var widgetId = string.IsNullOrEmpty(id) ? $"##{label}" : $"##{id}";
            return ImGui.SliderInt(widgetId, ref value, min, max);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }
}
