using Dalamud.Interface;
using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    public static bool MatchesFilter(string key, string label, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var trimmed = filter.Trim();
        if (string.Equals(key, trimmed, StringComparison.OrdinalIgnoreCase))
            return true;

        if (key.Contains(trimmed, StringComparison.OrdinalIgnoreCase))
            return true;

        return label.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
    }

    public static bool SearchFilter(
        ReadOnlySpan<byte> id,
        ref string filter,
        string? hint = null,
        int maxLength = 256,
        float width = -1f)
    {
        PushInputChrome();
        try
        {
            PushSearchFill();
            try
            {
                PushSearchIconPadding();
                try
                {
                    ImGui.SetNextItemWidth(width);
                    return ImGui.InputText(id, ref filter, maxLength, ImGuiInputTextFlags.AutoSelectAll);
                }
                finally
                {
                    PopSearchIconPadding(filter, hint);
                }
            }
            finally
            {
                PopSearchFill();
            }
        }
        finally
        {
            PopInputChrome();
        }
    }

    public static bool SearchFilter(
        string id,
        ref string filter,
        string? hint = null,
        int maxLength = 256,
        float width = -1f)
    {
        PushInputChrome();
        try
        {
            PushSearchFill();
            try
            {
                PushSearchIconPadding();
                try
                {
                    ImGui.SetNextItemWidth(width);
                    return ImGui.InputText(id, ref filter, maxLength, ImGuiInputTextFlags.AutoSelectAll);
                }
                finally
                {
                    PopSearchIconPadding(filter, hint);
                }
            }
            finally
            {
                PopSearchFill();
            }
        }
        finally
        {
            PopInputChrome();
        }
    }

    private static float _searchIconWidth;
    private static float _searchIconGap;
    private static float _searchPadX;
    private static Vector2 _searchTextPad;

    private static void PushSearchIconPadding()
    {
        var icon = FontAwesomeIcon.Search.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        _searchIconWidth = ImGui.CalcTextSize(icon).X;
        ImGui.PopFont();

        _searchIconGap = ImGui.GetStyle().ItemInnerSpacing.X;
        var pad = ImGui.GetStyle().FramePadding;
        _searchPadX = pad.X;
        _searchTextPad = new Vector2(pad.X + _searchIconWidth + _searchIconGap, pad.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, _searchTextPad);
    }

    private const float SearchBorderAlpha = 0.28f;

    private static void PushSearchFill()
    {
        var border = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).Separator;
        border.W = SearchBorderAlpha;
        var fill = ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg];
        fill.W = 0.58f;
        ImGui.PushStyleColor(ImGuiCol.FrameBg, fill);
        fill.W = 0.74f;
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, fill);
        fill.W = 0.88f;
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, fill);
        ImGui.PushStyleColor(ImGuiCol.Border, border);
    }

    private static void PopSearchFill()
    {
        ImGui.PopStyleColor(4);
    }

    private static void PopSearchIconPadding(string filter, string? hint)
    {
        ImGui.PopStyleVar();

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var color = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        var icon = FontAwesomeIcon.Search.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(icon);
        var iconPos = new Vector2(
            min.X + _searchTextPad.X - _searchIconGap - _searchIconWidth,
            min.Y + (max.Y - min.Y - iconSize.Y) * 0.5f);
        ImGui.GetWindowDrawList().AddText(iconPos, color, icon);
        ImGui.PopFont();

        if (filter.Length > 0)
            return;

        var label = string.IsNullOrEmpty(hint) ? I18n.Get("window.search") : hint;
        var textPos = new Vector2(min.X + _searchTextPad.X, min.Y + _searchTextPad.Y);
        var dl = ImGui.GetWindowDrawList();
        dl.PushClipRect(textPos, new Vector2(max.X - _searchPadX, max.Y), true);
        dl.AddText(textPos, color, label);
        dl.PopClipRect();
    }

}
