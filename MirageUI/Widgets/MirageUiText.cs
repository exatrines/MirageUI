using Dalamud.Interface;
using Dalamud.Utility;

namespace MirageUI;

public static partial class MirageUi
{
    public static void PaddedSeparator()
    {
        var itemSpacingHeight = MirageLayout.Style.ItemSpacing.Y;

        MirageLayout.Cursor.Y += itemSpacingHeight;
        ImGui.Separator();
        MirageLayout.Cursor.Y += itemSpacingHeight - 1;
    }

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
        var framePadding = ResolveInputFramePadding();
        var buttonPadding = new Vector2(
            framePadding.X + ButtonExtraPaddingX,
            framePadding.Y);
        Vector2 labelSize;
        using (PushFont(FontSize.Default))
            labelSize = ImGui.CalcTextSize(buttonLabel);

        var buttonSize = new Vector2(
            labelSize.X + buttonPadding.X * 2f,
            ResolveControlHeight());
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

    public static bool Checkbox(string label, ref bool value)
    {
        using (PushCheckStyle())
            return ImGui.Checkbox(label, ref value);
    }

    public static bool Radio(string label, ref int current, int value)
    {
        using (PushCheckStyle())
            return ImGui.RadioButton(label, ref current, value);
    }

    public static CheckboxGroupScope CheckboxGroup(string label, ref bool value)
    {
        var changed = Checkbox(label, ref value);
        ImGui.Indent();
        return new CheckboxGroupScope(changed);
    }

    private static ImRaii.StyleDisposable PushCheckStyle()
    {
        var pad = MirageLayout.Style.FramePadding;
        var padY = ResolveInputFramePadding().Y;
        return ImRaii
            .PushStyle(ImGuiStyleVar.FramePadding, new Vector2(pad.X, padY))
            .Push(ImGuiStyleVar.FrameRounding, InputStyle.FrameRounding);
    }

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

    public static void Warning(string message, float width = -1f) =>
        Banner(FontAwesomeIcon.ExclamationTriangle, GetColor(Color.Warning), width, "", message);

    public static void Warning(string title, string message, float width = -1f) =>
        Banner(FontAwesomeIcon.ExclamationTriangle, GetColor(Color.Warning), width, title, message);

    public static void Info(string message, float width = -1f) =>
        Banner(FontAwesomeIcon.InfoCircle, GetColor(Color.Info), width, "", message);

    public static void Info(string title, string message, float width = -1f) =>
        Banner(FontAwesomeIcon.InfoCircle, GetColor(Color.Info), width, title, message);

    public static void Info(string title, Action drawContent, float width = -1f) =>
        Banner(FontAwesomeIcon.InfoCircle, GetColor(Color.Info), width, title, drawContent);

    private static void Banner(
        FontAwesomeIcon icon,
        Vector4 accent,
        float width,
        string title,
        string message) =>
        Banner(icon, accent, width, title, () =>
        {
            ImGui.PushStyleColor(ImGuiCol.Text, GetColor(Color.Default));
            ImGui.TextWrapped(message);
            ImGui.PopStyleColor();
        });

    private static void Banner(
        FontAwesomeIcon icon,
        Vector4 accent,
        float width,
        string title,
        Action drawBody)
    {
        ArgumentNullException.ThrowIfNull(drawBody);

        var scale = MirageLayout.Style.Scale;
        var padding = 10f * scale;
        var gap = 8f * scale;
        var rounding = 4f * scale;
        var avail = width > 0f ? width : MirageLayout.Style.ContentRegionAvail.X;
        if (avail <= 0f)
            return;

        var bg = new Vector4(accent.X, accent.Y, accent.Z, 0.18f);
        var border = new Vector4(accent.X, accent.Y, accent.Z, 0.55f);

        ImGui.PushFont(UiBuilder.IconFont);
        var iconText = icon.ToIconString();
        var iconSize = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();

        var hasTitle = !string.IsNullOrWhiteSpace(title);
        var titleSize = hasTitle ? ImGui.CalcTextSize(title) : Vector2.Zero;
        var line = ImGui.GetTextLineHeight();
        var titleRow = hasTitle ? MathF.Max(iconSize.Y, titleSize.Y) : 0f;
        var contentWidth = MathF.Max(1f, avail - padding * 2f - iconSize.X - gap);
        var min = MirageLayout.Cursor.ScreenPosition;
        var contentX = min.X + padding + iconSize.X + gap;
        var bodyY = hasTitle
            ? min.Y + padding + titleRow + gap * 0.5f
            : min.Y + padding;
        var iconY = hasTitle
            ? min.Y + padding + (titleRow - iconSize.Y) * 0.5f
            : bodyY + (line - iconSize.Y) * 0.5f;

        var dl = ImGui.GetWindowDrawList();
        dl.ChannelsSplit(2);
        dl.ChannelsSetCurrent(1);

        if (hasTitle)
        {
            var titleY = min.Y + padding + (titleRow - titleSize.Y) * 0.5f;
            dl.AddText(new Vector2(contentX, titleY), ToUInt(accent), title);
        }

        MirageLayout.Cursor.ScreenPosition = new Vector2(contentX, bodyY);
        ImGui.PushItemWidth(contentWidth);
        ImGui.PushTextWrapPos(contentX + contentWidth);
        drawBody();
        ImGui.PopTextWrapPos();
        ImGui.PopItemWidth();

        var contentBottom = ImGui.GetItemRectMax().Y;
        contentBottom = MathF.Max(contentBottom, iconY + iconSize.Y);
        var max = new Vector2(min.X + avail, contentBottom + padding);

        dl.ChannelsSetCurrent(0);
        dl.AddRectFilled(min, max, ToUInt(bg), rounding);
        dl.AddRect(min, max, ToUInt(border), rounding);

        var iconPos = new Vector2(min.X + padding, iconY);
        ImGui.PushFont(UiBuilder.IconFont);
        dl.AddText(iconPos, ToUInt(accent), iconText);
        ImGui.PopFont();
        dl.ChannelsMerge();

        MirageLayout.Cursor.ScreenPosition = new Vector2(min.X, max.Y);
        MirageLayout.Cursor.Y += MirageLayout.Style.ItemSpacing.Y;
    }

    public static bool Link(string label, string url, Color color = Color.Accent, bool emphasized = false)
    {
        var enabled = !string.IsNullOrWhiteSpace(url);
        var textColor = enabled ? GetColor(color) : GetColor(Color.Secondary);
        var hover = new Vector4(
            MathF.Min(textColor.X + 0.15f, 1f),
            MathF.Min(textColor.Y + 0.15f, 1f),
            MathF.Min(textColor.Z + 0.15f, 1f),
            textColor.W);

        using (PushFont(FontSize.Default))
        {
            var icon = FontAwesomeIcon.Link.ToIconString();
            Vector2 iconSize;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                iconSize = ImGui.CalcTextSize(icon);

            var gap = ImGui.GetStyle().ItemInnerSpacing.X;
            var textSize = ImGui.CalcTextSize(label);
            var height = MathF.Max(iconSize.Y, textSize.Y);
            var width = iconSize.X + gap + textSize.X;
            var pos = ImGui.GetCursorScreenPos();
            var pressed = ImGui.InvisibleButton($"##{label}/{url}", new Vector2(width, height));
            var hovered = enabled && ImGui.IsItemHovered();
            if (hovered)
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            var colorNow = hovered ? hover : textColor;
            var iconY = pos.Y + (height - iconSize.Y) * 0.5f;
            var textY = pos.Y + (height - textSize.Y) * 0.5f;
            var textX = pos.X + iconSize.X + gap;
            var drawList = ImGui.GetWindowDrawList();
            var packed = ToUInt(colorNow);

            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                drawList.AddText(
                    ImGui.GetFont(),
                    ImGui.GetFontSize(),
                    new Vector2(pos.X, iconY),
                    packed,
                    icon);
            }

            drawList.AddText(new Vector2(textX, textY), packed, label);
            var scale = MirageLayout.Style.Scale;
            var underlineY = textY + textSize.Y - scale;
            drawList.AddLine(
                new Vector2(textX, underlineY),
                new Vector2(textX + textSize.X, underlineY),
                packed,
                emphasized ? 2f * scale : 1f);

            if (pressed && enabled)
                Util.OpenLink(url);

            return pressed && enabled;
        }
    }

    internal static float GetLinkButtonHeight()
    {
        using (PushFont(FontSize.Default))
        {
            float icon;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                icon = ImGui.GetTextLineHeight();
            return MathF.Max(ImGui.GetTextLineHeight(), icon);
        }
    }

    internal static float MeasureLinkWidth(string label)
    {
        using (PushFont(FontSize.Default))
        {
            float icon;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                icon = ImGui.CalcTextSize(FontAwesomeIcon.Link.ToIconString()).X;
            return icon + ImGui.GetStyle().ItemInnerSpacing.X + ImGui.CalcTextSize(label).X;
        }
    }

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
