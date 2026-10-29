using MirageUI.Theme;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    public static bool BeginAccentPopup(string popupId)
    {
        var settings = MirageTheme.Active ?? ResolveAppliedColors();
        var accent = settings.GetColor(Color.Accent);
        var scale = MirageLayout.Style.Scale;
        var popupBg = MixColors(settings.WindowBg, accent, 0.08f);
        popupBg.W = 0.96f;

        ImGui.PushStyleColor(ImGuiCol.PopupBg, popupBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, WithAlpha(accent, 0.14f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, WithAlpha(accent, 0.26f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, WithAlpha(accent, 0.38f));
        ImGui.PushStyleColor(ImGuiCol.Border, WithAlpha(accent, 0.75f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark, accent);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 4f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6f * scale, 6f * scale));

        if (ImGui.BeginPopup(popupId))
            return true;

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(6);
        return false;
    }

    public static void EndAccentPopup()
    {
        ImGui.EndPopup();
        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(6);
    }

    public static bool DrawChecklistPopupContents(
        IReadOnlyList<MirageChecklistOption> options,
        HashSet<string> selectedIds,
        MirageChecklistMode mode)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(selectedIds);

        var changed = false;
        foreach (var option in options)
        {
            var selected = selectedIds.Contains(option.Id);
            if (!Checkbox(option.Label, ref selected))
                continue;

            changed = true;
            if (mode == MirageChecklistMode.Unique)
            {
                selectedIds.Clear();
                if (selected)
                    selectedIds.Add(option.Id);
            }
            else if (selected)
            {
                selectedIds.Add(option.Id);
            }
            else
            {
                selectedIds.Remove(option.Id);
            }
        }

        return changed;
    }

    public static bool FilterIconPopup(
        string popupId,
        IReadOnlyList<MirageChecklistOption> options,
        HashSet<string> selectedIds,
        MirageChecklistMode mode = MirageChecklistMode.Unique)
    {
        if (!BeginAccentPopup(popupId))
            return false;

        var changed = DrawChecklistPopupContents(options, selectedIds, mode);
        EndAccentPopup();
        return changed;
    }
}
