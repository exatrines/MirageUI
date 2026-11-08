using Dalamud.Interface;
using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    public static void DrawThemePage() => MirageThemePage.Draw();
}

internal static class MirageThemePage
{
    private static bool _confirmDelete;

    internal static void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16f, 12f) * scale);
        var open = ImGui.BeginChild("##MirageThemePage", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.AlwaysUseWindowPadding);
        ImGui.PopStyleVar();
        try
        {
            if (open)
                DrawBody();
        }
        finally
        {
            ImGui.EndChild();
        }

        var selected = MirageThemes.Selected;
        MirageUi.Message.Draw(
            ref _confirmDelete,
            MirageUi.I18n.Get("theme.delete.confirm"),
            selected?.Name ?? "",
            FontAwesomeIcon.ExclamationTriangle,
            new MirageMessageButton { Label = MirageUi.I18n.Get("theme.cancel") },
            new MirageMessageButton
            {
                Label = MirageUi.I18n.Get("theme.delete"),
                Primary = true,
                OnClick = MirageThemes.DeleteSelected,
            });
    }

    internal static void Reset() => _confirmDelete = false;

    private static void DrawBody()
    {
        if (MirageThemes.Selected == null)
            return;

        MirageUi.Text(MirageUi.I18n.Get("theme.title"), MirageUi.Color.Title, fontSize: MirageUi.FontSize.Large);
        DrawToolbar(ImGuiHelpers.GlobalScale);

        var locked = MirageThemes.IsBuiltin(MirageThemes.SelectedId);
        if (locked)
            ImGui.BeginDisabled();

        var theme = MirageThemes.Current;
        EditColor(MirageUi.I18n.Get("theme.main"), theme.GetColor(MirageUi.Color.Title), false, value =>
        {
            value.W = 1f;
            theme.SetColor(MirageUi.Color.Title, value);
        });
        EditColor(MirageUi.I18n.Get("theme.accent"), theme.GetColor(MirageUi.Color.Accent), false, theme.ApplyAccent);
        EditColor(MirageUi.I18n.Get("theme.text"), theme.GetColor(MirageUi.Color.Default), false, value =>
        {
            value.W = 1f;
            theme.SetColor(MirageUi.Color.Default, value);
        });
        EditColor(MirageUi.I18n.Get("theme.background"), theme.WindowBg, true, value =>
        {
            theme.WindowBg = value;
            theme.TitleBg = value;
        });
        EditColor(MirageUi.I18n.Get("theme.panels"), theme.ChromeBg, true, value => theme.ChromeBg = value);
        EditColor(MirageUi.I18n.Get("theme.page"), theme.PageBg, true, value => theme.PageBg = value);
        EditColor(MirageUi.I18n.Get("theme.separator"), theme.Separator, true, value => theme.Separator = value);

        var blur = theme.BlurStrength;
        if (MirageUi.SliderFloat(MirageUi.I18n.Get("theme.blur"), ref blur, 0f, MirageColorSettings.MaxBlurStrength, "%.0f"))
        {
            theme.BlurStrength = blur;
            MirageThemes.SaveCurrent();
        }

        var size = theme.UiFontSizePt;
        if (MirageUi.SliderFloat(MirageUi.I18n.Get("theme.size"), ref size, 10f, 18f, "%.0f pt"))
        {
            theme.UiFontSizePt = size;
            MirageThemes.SaveCurrent();
        }

        if (locked)
            ImGui.EndDisabled();
    }

    private static void EditColor(string label, Vector4 color, bool alpha, Action<Vector4> apply)
    {
        var flags = alpha ? ImGuiColorEditFlags.AlphaBar : ImGuiColorEditFlags.NoAlpha;
        if (!MirageUi.ColorEdit4(label, ref color, flags))
            return;

        apply(color);
        MirageThemes.SaveCurrent();
    }

    private static void DrawToolbar(float scale)
    {
        var gap = ImGui.GetStyle().ItemSpacing.X;
        var button = MirageUi.ResolveControlHeight();
        var trailing = button * 5f + gap * 6f + 1f;
        var dropdownWidth = MathF.Max(120f * scale, ImGui.GetContentRegionAvail().X - trailing);
        var selected = MirageThemes.Selected!;
        if (MirageUi.BeginDropdown("", MirageThemes.DisplayName(selected), "themePick", dropdownWidth))
        {
            foreach (var theme in MirageThemes.All)
            {
                var current = theme.Id == MirageThemes.SelectedId;
                if (ImGui.Selectable($"{MirageThemes.DisplayName(theme)}##{theme.Id}", current) && !current)
                    MirageThemes.Select(theme.Id);
                if (current)
                    ImGui.SetItemDefaultFocus();
            }

            MirageUi.EndDropdown();
        }

        ImGui.SameLine(0f, gap);
        if (MirageUi.IconButton(FontAwesomeIcon.Plus, id: "themeAdd", tooltip: MirageUi.I18n.Get("theme.add")))
            MirageThemes.Add();

        ImGui.SameLine(0f, gap);
        if (MirageUi.IconButton(FontAwesomeIcon.Copy, id: "themeDuplicate", tooltip: MirageUi.I18n.Get("theme.duplicate")))
            MirageThemes.DuplicateSelected();

        ImGui.SameLine(0f, gap);
        var builtin = MirageThemes.IsBuiltin(MirageThemes.SelectedId);
        if (MirageUi.IconButton(
                FontAwesomeIcon.Trash,
                id: "themeDelete",
                tooltip: MirageUi.I18n.Get(builtin ? "theme.delete.builtin" : "theme.delete"),
                enabled: !builtin))
            _confirmDelete = true;

        ImGui.SameLine(0f, gap);
        var lineTop = ImGui.GetCursorScreenPos();
        var inset = 4f * scale;
        ImGui.GetWindowDrawList().AddLine(
            lineTop + new Vector2(0f, inset),
            lineTop + new Vector2(0f, button - inset),
            ImGui.GetColorU32(ImGuiCol.Separator));
        ImGui.Dummy(new Vector2(1f, button));

        ImGui.SameLine(0f, gap);
        if (MirageUi.IconButton(FontAwesomeIcon.FileExport, id: "themeExport", tooltip: MirageUi.I18n.Get("theme.export")))
            MirageThemes.ExportSelected();

        ImGui.SameLine(0f, gap);
        if (MirageUi.IconButton(FontAwesomeIcon.FileImport, id: "themeImport", tooltip: MirageUi.I18n.Get("theme.import")))
            MirageThemes.ImportClipboard();
    }
}
