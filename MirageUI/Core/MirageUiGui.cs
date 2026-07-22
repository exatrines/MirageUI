using Dalamud.Utility;
using MirageUI.Theme;
using MirageUI.Ui;

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

    public static bool SearchFilter(ReadOnlySpan<byte> id, ref string filter, string hint, int maxLength = 256)
    {
        ImGui.SetNextItemWidth(-1f);
        return ImGui.InputTextWithHint(id, hint, ref filter, maxLength, ImGuiInputTextFlags.AutoSelectAll);
    }

    public static void PaddedSeparator()
    {
        var itemSpacingHeight = MirageLayout.Style.ItemSpacing.Y;

        MirageLayout.Cursor.Y += itemSpacingHeight;
        ImGui.Separator();
        MirageLayout.Cursor.Y += itemSpacingHeight - 1;
    }

    /// <summary>大きい見出し + 下線。</summary>
    public static void Header(
        string title,
        Color color = Color.Title,
        FontSize fontSize = FontSize.Large,
        bool underline = true)
    {
        MirageLayout.Cursor.Y += 5f * MirageLayout.Style.Scale;

        using (PushFont(fontSize))
            DrawText(title, GetColor(color), wrap: false);

        if (underline)
            PaddedSeparator();
    }

    /// <summary>Title with trailing On/Off status + underline.</summary>
    public static void HeaderWithBool(
        string title,
        bool value,
        string trueText = "ON",
        string falseText = "OFF",
        Color color = Color.Title,
        FontSize fontSize = FontSize.Large,
        bool underline = true)
    {
        MirageLayout.Cursor.Y += 5f * MirageLayout.Style.Scale;

        var trailing = value ? trueText : falseText;

        using (PushFont(fontSize))
        {
            DrawText(title, GetColor(color), wrap: false);

            var windowX = MirageLayout.Style.ContentRegionMax.X;
            var textSize = ImGui.CalcTextSize(trailing);
            ImGui.SameLine(windowX - textSize.X);

            if (value)
                ThemeColors.TextAccent(trailing);
            else
                ThemeColors.TextSecondary(trailing);
        }

        if (underline)
            PaddedSeparator();
    }

    /// <summary>
    /// Title with a single Start/Stop toggle on the right.
    /// Returns true when the toggle was clicked.
    /// </summary>
    public static bool HeaderWithRunToggle(
        string title,
        bool isRunning,
        string startText = "Start",
        string stopText = "Stop",
        Color color = Color.Title,
        FontSize fontSize = FontSize.Large,
        bool underline = true,
        string id = "header-run")
    {
        var scale = MirageLayout.Style.Scale;
        MirageLayout.Cursor.Y += 5f * scale;

        var startCursor = MirageLayout.Cursor.Position;
        float titleHeight;
        using (PushFont(fontSize))
        {
            DrawText(title, GetColor(color), wrap: false);
            titleHeight = ImGui.GetItemRectSize().Y;
        }

        var buttonLabel = isRunning ? stopText : startText;
        var framePadding = MirageLayout.Style.FramePadding;
        var buttonPadding = new Vector2(
            framePadding.X + ButtonExtraPaddingX,
            framePadding.Y + ButtonExtraPaddingY);
        Vector2 labelSize;
        using (PushFont(FontSize.Default))
            labelSize = ImGui.CalcTextSize(buttonLabel);

        var buttonSize = new Vector2(
            labelSize.X + buttonPadding.X * 2f,
            labelSize.Y + buttonPadding.Y * 2f);
        var buttonX = MirageLayout.Style.ContentRegionMax.X - buttonSize.X;
        var buttonY = startCursor.Y + MathF.Max(0f, (titleHeight - buttonSize.Y) * 0.5f);
        MirageLayout.Cursor.Position = new Vector2(buttonX, buttonY);

        var clicked = isRunning
            ? SecondaryButton(buttonLabel, id: id)
            : PrimaryButton(buttonLabel, id: id);

        // Keep layout flow below the taller of title / button.
        var rowBottom = startCursor.Y + MathF.Max(titleHeight, buttonSize.Y);
        MirageLayout.Cursor.Position = new Vector2(startCursor.X, rowBottom);

        if (underline)
            PaddedSeparator();

        return clicked;
    }

    /// <summary>Section heading inside settings + underline.</summary>
    public static void SubHeader(
        string label,
        bool pushDown = true,
        Color color = Color.Title,
        FontSize fontSize = FontSize.Default,
        bool underline = true)
    {
        var itemSpacingHeight = MirageLayout.Style.ItemSpacing.Y;

        if (pushDown)
            MirageLayout.Cursor.Y += itemSpacingHeight * 2;

        using (PushFont(fontSize))
            DrawText(label, GetColor(color), wrap: false);

        if (underline)
        {
            MirageLayout.Cursor.Y += -itemSpacingHeight + 3;
            ImGui.Separator();
            MirageLayout.Cursor.Y += itemSpacingHeight * 2 - 1;
        }
    }

    /// <summary>チェックボックス。</summary>
    public static bool Checkbox(string label, ref bool value) =>
        ImGui.Checkbox(label, ref value);

    /// <summary>
    /// チェックボックスの直下から <see cref="CheckboxGroupScope"/> の Dispose までをインデントする。
    /// 説明文や子コントロールは <c>using</c> ブロック内に描画する。
    /// </summary>
    public static CheckboxGroupScope CheckboxGroup(string label, ref bool value)
    {
        var changed = ImGui.Checkbox(label, ref value);
        ImGui.Indent();
        return new CheckboxGroupScope(changed);
    }

    /// <summary>本文。<paramref name="color"/> は <see cref="Color"/> または Vector4。</summary>
    public static void Text(
        string text,
        Color color = Color.Default,
        bool wrap = true,
        bool spaced = false,
        bool centered = false,
        FontSize fontSize = FontSize.Default,
        bool underline = false)
    {
        using (PushFont(fontSize))
            DrawText(text, GetColor(color), wrap, spaced, centered, underline);
    }

    public static void Text(
        string text,
        Vector4 customColor,
        bool wrap = true,
        bool spaced = false,
        bool centered = false,
        FontSize fontSize = FontSize.Default,
        bool underline = false)
    {
        using (PushFont(fontSize))
            DrawText(text, customColor, wrap, spaced, centered, underline);
    }

    public static void OverlayFill(Vector2 screenPos, Vector2 size, float rounding, ImDrawFlags flags = ImDrawFlags.None)
    {
        ImGui.GetWindowDrawList().AddRectFilled(
            screenPos,
            screenPos + size,
            PanelOverlay,
            rounding,
            flags);
    }

    /// <summary>ラベル付きリンクボタン。クリックで URL を開く。</summary>
    public static bool Link(string label, string url, Color color = Color.Accent)
    {
        var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
        var (button, buttonHovered, buttonActive, text, border) = GetLinkButtonColors(settings, color);
        var framePadding = MirageLayout.Style.FramePadding;

        using (PushFont(FontSize.Default))
        {
            using var colors = ImRaii
                .PushColor(ImGuiCol.Button, button)
                .Push(ImGuiCol.ButtonHovered, buttonHovered)
                .Push(ImGuiCol.ButtonActive, buttonActive)
                .Push(ImGuiCol.Text, text)
                .Push(ImGuiCol.Border, border);
            using var styles = ImRaii
                .PushStyle(ImGuiStyleVar.FrameRounding, LinkButtonFrameRounding)
                .Push(ImGuiStyleVar.FrameBorderSize, 1f)
                .Push(ImGuiStyleVar.FramePadding, GetLinkButtonFramePadding(framePadding));

            var clicked = ImGui.Button(label);
            if (clicked && !string.IsNullOrWhiteSpace(url))
                Util.OpenLink(url);

            return clicked;
        }
    }

    internal static float GetLinkButtonHeight()
    {
        using (PushFont(FontSize.Default))
            return ImGui.GetTextLineHeight() + LinkButtonExtraPaddingY * 2f + 2f;
    }

    internal static float GetLinkButtonHorizontalPadding() =>
        MirageLayout.Style.FramePadding.X + LinkButtonExtraPaddingX;

    private static Vector2 GetLinkButtonFramePadding(Vector2 framePadding) =>
        new(framePadding.X + LinkButtonExtraPaddingX, LinkButtonExtraPaddingY);

    private const float LinkButtonExtraPaddingX = 6f;
    private const float LinkButtonExtraPaddingY = 3f;
    private const float LinkButtonFrameRounding = 3f;

    private static (Vector4 Button, Vector4 Hovered, Vector4 Active, Vector4 Text, Vector4 Border) GetLinkButtonColors(
        MirageColorSettings settings,
        Color textColor)
    {
        var accent = settings.GetColor(Color.Accent);
        var text = settings.GetColor(textColor);
        var baseBg = WithAlpha(accent, 0.16f);
        var hovered = WithAlpha(accent, 0.28f);
        var active = WithAlpha(accent, 0.40f);
        return (baseBg, hovered, active, text, accent);
    }

    private static Vector4 WithAlpha(Vector4 color, float alpha) =>
        new(color.X, color.Y, color.Z, alpha);

    private static void DrawText(
        string text,
        Vector4 color,
        bool wrap,
        bool spaced = false,
        bool centered = false,
        bool underline = false)
    {
        if (spaced)
            MirageLayout.Cursor.Y += MirageLayout.Style.ItemSpacing.Y;

        if (centered && !wrap)
        {
            var cursorPos = ImGui.GetCursorPos();
            var contentAvail = MirageLayout.Style.ContentRegionAvail;
            var textSize = ImGui.CalcTextSize(text);
            MirageLayout.Cursor.Position = cursorPos + contentAvail / 2 - textSize / 2;
        }

        var drawPos = ImGui.GetCursorScreenPos();

        if (wrap)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(text);
            ImGui.PopStyleColor();

            if (underline)
            {
                var min = ImGui.GetItemRectMin();
                var max = ImGui.GetItemRectMax();
                DrawTextUnderline(new Vector2(min.X, min.Y), new Vector2(max.X - min.X, max.Y - min.Y), color);
            }
        }
        else
        {
            ImGui.TextColored(color, text);

            if (underline)
            {
                var textSize = ImGui.CalcTextSize(text);
                DrawTextUnderline(drawPos, textSize, color);
            }
        }

        if (spaced)
            MirageLayout.Cursor.Y += MirageLayout.Style.ItemSpacing.Y;
    }

    private static void DrawTextUnderline(Vector2 screenPos, Vector2 textSize, Vector4 color)
    {
        var offset = 1f * MirageLayout.Style.Scale;
        var y = screenPos.Y + textSize.Y - offset;
        ImGui.GetWindowDrawList().AddLine(
            new Vector2(screenPos.X, y),
            new Vector2(screenPos.X + textSize.X, y),
            ToUInt(color),
            1f);
    }
}
