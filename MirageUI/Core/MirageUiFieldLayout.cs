using Dalamud.Interface;
using MirageUI.Theme;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>Fixed width of the left label column for field rows.</summary>
    public const float FieldLabelColumnWidth = 128f;

    private static readonly Vector4 MatchCheckGreen = new(0.45f, 0.85f, 0.45f, 1f);
    private static readonly Vector4 MatchFailRed = new(0.9f, 0.35f, 0.35f, 1f);

    /// <summary>
    /// Starts a 2-column field row: left label, right control.
    /// When <paramref name="label"/> is empty, draws no table (control-only).
    /// When <paramref name="liveMatch"/> is set, draws a check/times icon to the left of the control.
    /// </summary>
    private static bool BeginFieldRow(
        string label,
        string id,
        out bool usedTable,
        bool? liveMatch = null,
        string? matchTooltip = null)
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
        if (!ImGui.BeginTable(
                tableId,
                2,
                ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.NoPadOuterX,
                new Vector2(-1f, 0f)))
            return false;

        ImGui.TableSetupColumn("##label", ImGuiTableColumnFlags.WidthFixed, FieldLabelColumnWidth);
        ImGui.TableSetupColumn("##field", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(label);

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
