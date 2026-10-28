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
        var rounding = 0f;

        if (position == MirageTooltipPosition.Right && anchor is { } sized)
        {
            var height = sized.GetHeight();
            padding.X = 10f * scale;
            padding.Y = MathF.Max(0f, (height - textSize.Y) * 0.5f);
            windowSize = new Vector2(textSize.X + padding.X * 2f, height);
            rounding = height * 0.22f;
        }

        if (position != MirageTooltipPosition.Cursor)
        {
            var rect = anchor ?? new ImRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax());
            var gap = AnchorGapPx * scale;
            var pos = position switch
            {
                MirageTooltipPosition.Above => new Vector2(
                    rect.Min.X + (rect.GetWidth() - windowSize.X) * 0.5f,
                    rect.Min.Y - windowSize.Y - gap),
                MirageTooltipPosition.Below => new Vector2(
                    rect.Min.X + (rect.GetWidth() - windowSize.X) * 0.5f,
                    rect.Max.Y + gap),
                _ => new Vector2(
                    rect.Max.X + gap,
                    rect.Min.Y + (rect.GetHeight() - windowSize.Y) * 0.5f),
            };
            ImGui.SetNextWindowPos(pos, ImGuiCond.Always);
        }

        ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, padding);
        using var roundingStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowRounding, rounding > 0f ? rounding : ImGui.GetStyle().WindowRounding);
        ImGui.BeginTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }
}
