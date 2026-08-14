using Dalamud.Interface;
using MirageUI.Theme;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>Default fixed width of the left label column for field rows.</summary>
    public const float FieldLabelColumnWidth = 128f;

    private static readonly Vector4 MatchCheckGreen = new(0.45f, 0.85f, 0.45f, 1f);
    private static readonly Vector4 MatchFailRed = new(0.9f, 0.35f, 0.35f, 1f);

    /// <summary>
    /// Starts a field row that spans the full content width.
    /// Left label is left-aligned; right control is right-aligned when its width is fixed.
    /// <paramref name="labelWidth"/> &gt; 0 and <paramref name="controlWidth"/> &gt; 0 pin those
    /// sides; leftover space sits in the middle. Either side may stretch instead (width &lt;= 0).
    /// Empty <paramref name="label"/> draws no table (control only).
    /// </summary>
    private static bool BeginFieldRow(
        string label,
        string id,
        out bool usedTable,
        bool? liveMatch = null,
        string? matchTooltip = null,
        float labelWidth = FieldLabelColumnWidth,
        float controlWidth = 0f)
    {
        usedTable = false;
        if (string.IsNullOrEmpty(label))
        {
            if (liveMatch is { } matched)
            {
                ImGui.AlignTextToFramePadding();
                DrawMatchStatusIcon(matched, matchTooltip);
                ImGui.SameLine(0f, ImGui.GetStyle().ItemInnerSpacing.X);
            }

            return true;
        }

        var tableId = string.IsNullOrEmpty(id) ? $"##MirageField_{label}" : $"##MirageField_{id}";
        var labelFixed = labelWidth > 0f;
        var controlFixed = controlWidth > 0f;
        if (!labelFixed && !controlFixed)
        {
            labelFixed = true;
            labelWidth = FieldLabelColumnWidth;
        }

        var columnCount = labelFixed && controlFixed ? 3 : 2;
        if (!ImGui.BeginTable(
                tableId,
                columnCount,
                ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.NoPadOuterX,
                new Vector2(-1f, 0f)))
            return false;

        if (labelFixed)
            ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthFixed, labelWidth);
        else
            ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthStretch);

        if (columnCount == 3)
            ImGui.TableSetupColumn("##gap", ImGuiTableColumnFlags.WidthStretch);

        if (controlFixed)
            ImGui.TableSetupColumn("##field", ImGuiTableColumnFlags.WidthFixed, controlWidth);
        else
            ImGui.TableSetupColumn("##field", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);

        if (columnCount == 3)
            ImGui.TableNextColumn();

        ImGui.TableNextColumn();
        usedTable = true;
        if (liveMatch is { } m)
        {
            ImGui.AlignTextToFramePadding();
            DrawMatchStatusIcon(m, matchTooltip);
            ImGui.SameLine(0f, ImGui.GetStyle().ItemInnerSpacing.X);
        }

        return true;
    }

    private static void DrawMatchStatusIcon(bool matched, string? tooltip)
    {
        var icon = matched ? FontAwesomeIcon.Check : FontAwesomeIcon.Times;
        var color = matched ? MatchCheckGreen : MatchFailRed;
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopStyleColor();
        ImGui.PopFont();
        if (!string.IsNullOrEmpty(tooltip))
            Tooltip(tooltip);
    }

    private static void EndFieldRow(bool usedTable)
    {
        if (usedTable)
            ImGui.EndTable();
    }
}
