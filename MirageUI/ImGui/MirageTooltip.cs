namespace MirageUI.Ui;

internal static class MirageTooltip
{
    private const float PaddingPx = 5f;
    private const float AnchorGapPx = 4f;

    internal static void Show(string text, MirageTooltipPosition position, ImRect? anchor = null)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var scale = MirageLayout.Style.Scale;
        var padding = new Vector2(PaddingPx * scale);
        var textSize = ImGui.CalcTextSize(text);
        var windowSize = textSize + padding * 2f;

        if (position is MirageTooltipPosition.Above or MirageTooltipPosition.Below)
        {
            var rect = anchor ?? new ImRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
            var gap = AnchorGapPx * scale;
            var x = rect.Min.X + (rect.GetWidth() - windowSize.X) * 0.5f;
            var y = position == MirageTooltipPosition.Above
                ? rect.Min.Y - windowSize.Y - gap
                : rect.Max.Y + gap;
            ImGui.SetNextWindowPos(new Vector2(x, y), ImGuiCond.Always);
        }

        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, padding);
        ImGui.BeginTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }
}
