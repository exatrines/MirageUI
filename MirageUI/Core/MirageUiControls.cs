namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>コンボボックス。</summary>
    public static bool Combo(
        string label,
        ref int currentItem,
        string[] items,
        float width = -1f,
        string? hint = null,
        Color hintColor = Color.Secondary)
    {
        SetControlWidth(width);
        var changed = ImGui.Combo(label, ref currentItem, items, items.Length);
        DrawControlHint(hint, hintColor);
        return changed;
    }

    /// <summary>整数スライダー。</summary>
    public static bool SliderInt(
        string label,
        ref int value,
        int min,
        int max,
        float width = -1f,
        string? hint = null,
        Color hintColor = Color.Secondary)
    {
        SetControlWidth(width);
        var changed = ImGui.SliderInt(label, ref value, min, max);
        DrawControlHint(hint, hintColor);
        return changed;
    }

    /// <summary>浮動小数スライダー。</summary>
    public static bool SliderFloat(
        string label,
        ref float value,
        float min,
        float max,
        float width = -1f,
        string format = "%.1f",
        string? hint = null,
        Color hintColor = Color.Secondary)
    {
        value = Math.Clamp(value, min, max);
        SetControlWidth(width);
        var changed = ImGui.SliderFloat(label, ref value, min, max, format);
        DrawControlHint(hint, hintColor);
        return changed;
    }

    /// <summary><paramref name="disabled"/> が true の間、子要素を無効化する。</summary>
    public static DisabledScope DisabledIf(bool disabled)
    {
        if (disabled)
            ImGui.BeginDisabled();

        return new DisabledScope(disabled);
    }

    /// <summary>検索付きコンボボックスの popup 内容を描画する。</summary>
    public static bool SearchCombo(
        string label,
        string preview,
        float width,
        ref string searchFilter,
        ReadOnlySpan<byte> searchId,
        string searchHint,
        Action? onPopupOpened,
        Action drawItems)
    {
        ImGui.AlignTextToFramePadding();
        SetControlWidth(width);

        if (!ImGui.BeginCombo(label, preview, ImGuiComboFlags.HeightLarge))
            return false;

        if (ImGui.IsWindowAppearing())
            onPopupOpened?.Invoke();

        SearchFilter(searchId, ref searchFilter, searchHint);
        drawItems();
        ImGui.EndCombo();
        return true;
    }

    private static void SetControlWidth(float width) =>
        ImGui.SetNextItemWidth(width > 0f ? width : -1f);

    private static void DrawControlHint(string? hint, Color hintColor)
    {
        if (string.IsNullOrEmpty(hint))
            return;

        ImGui.Indent();
        Text(hint, color: hintColor, wrap: true);
        ImGui.Unindent();
    }
}
