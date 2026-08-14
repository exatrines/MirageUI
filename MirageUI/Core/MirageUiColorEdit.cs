namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>
    /// Accent-themed color editor. Layout: [label | color] via Table API.
    /// Empty <paramref name="label"/> = control only.
    /// </summary>
    public static bool ColorEdit4(
        string label,
        ref Vector4 color,
        ImGuiColorEditFlags flags = ImGuiColorEditFlags.AlphaBar,
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
            return ImGui.ColorEdit4(widgetId, ref color, flags);
        }
        finally
        {
            PopInputChrome();
            EndFieldRow(usedTable);
        }
    }
}
