using Dalamud.Interface;
using MirageUI.Theme;

namespace MirageUI;

public static partial class MirageUi
{
    public static class Message
    {
        private const float PanelRounding = 6f;
        private const float PanelMinWidth = 280f;
        private const float PanelMaxWidthRatio = 0.72f;
        private const float PreferredWidth = 360f;
        private const float ContentPadding = 18f;
        private const float SectionSpacing = 12f;
        private const float ButtonGap = 8f;
        private const float ButtonMinWidth = 88f;
        private const int MaxButtons = 3;
        private static readonly Vector4 OverlayVeil = new(0.12f, 0.12f, 0.12f, 0.62f);

        /// <summary>
        /// 画面中央の Message ダイアログ。<see cref="MirageMessageState.Visible"/> で表示切替。
        /// ウィンドウ全体に半透明グレーをかぶせ、その上にメッセージを表示する。
        /// ホストウィンドウの Draw 末尾で呼ぶ。
        /// </summary>
        public static void Draw(MirageMessageState state)
        {
            if (!state.Visible)
                return;

            var visible = true;
            DrawCore(ref visible, state.Icon, state.Title, state.Message, state.Buttons);
            state.Visible = visible;
        }

        /// <summary>
        /// 画面中央の Message ダイアログ。<paramref name="visible"/> で表示切替。
        /// ウィンドウ全体に半透明グレーをかぶせ、その上にメッセージを表示する。
        /// ホストウィンドウの Draw 末尾で呼ぶ。
        /// </summary>
        public static void Draw(
            ref bool visible,
            string title,
            string message,
            FontAwesomeIcon? icon = null,
            params MirageMessageButton[] buttons)
        {
            if (!visible)
                return;

            DrawCore(ref visible, icon, title, message, buttons);
        }

        private static void DrawCore(
            ref bool visible,
            FontAwesomeIcon? icon,
            string title,
            string message,
            IList<MirageMessageButton> buttons)
        {
            var scale = MirageLayout.Style.Scale;
            var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
            var windowPos = ImGui.GetWindowPos();
            var windowSize = ImGui.GetWindowSize();
            if (windowSize.X <= 0f || windowSize.Y <= 0f)
                return;

            // 1) ウィンドウ全体に半透明グレーのレイヤーをかぶせる
            ImGui.SetCursorScreenPos(windowPos);
            using (ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 0f))
            using (ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero))
            using (ImRaii.PushColor(ImGuiCol.ChildBg, OverlayVeil))
            using (var overlay = ImRaii.Child(
                       "##MirageMessageOverlay"u8,
                       windowSize,
                       false,
                       ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (!overlay)
                    return;

                // 2) その上にメッセージパネルを中央配置
                var padding = ContentPadding * scale;
                var panelWidth = Math.Clamp(
                    PreferredWidth * scale,
                    PanelMinWidth * scale,
                    windowSize.X * PanelMaxWidthRatio);
                var innerWidth = panelWidth - padding * 2f;

                var titleHeight = MeasureTitleHeight(icon, title, innerWidth, scale);
                var messageHeight = MeasureMessageHeight(message, innerWidth);
                var hasMessage = !string.IsNullOrEmpty(message);
                var hasButtons = buttons.Count > 0;
                var buttonRowHeight = hasButtons ? MirageLayout.Style.FrameHeight + 4f * scale : 0f;

                var gapCount = 0;
                if (hasMessage)
                    gapCount++;
                if (hasButtons)
                    gapCount++;

                var panelHeight = padding * 2f
                    + titleHeight
                    + messageHeight
                    + buttonRowHeight
                    + SectionSpacing * scale * gapCount;
                panelHeight = Math.Min(panelHeight, windowSize.Y * 0.85f);

                var margin = 8f * scale;
                var panelPos = new Vector2(
                    Math.Clamp((windowSize.X - panelWidth) * 0.5f, margin, Math.Max(margin, windowSize.X - panelWidth - margin)),
                    Math.Clamp((windowSize.Y - panelHeight) * 0.5f, margin, Math.Max(margin, windowSize.Y - panelHeight - margin)));

                var accent = settings.GetColor(Color.Accent);
                var rounding = PanelRounding * scale;
                var panelScreenMin = ImGui.GetWindowPos() + panelPos;
                var panelScreenMax = panelScreenMin + new Vector2(panelWidth, panelHeight);
                var dl = ImGui.GetWindowDrawList();
                dl.AddRectFilled(panelScreenMin, panelScreenMax, ToUInt(settings.WindowBg), rounding);
                dl.AddRect(
                    panelScreenMin,
                    panelScreenMax,
                    ToUInt(WithAlpha(accent, 0.85f)),
                    rounding,
                    ImDrawFlags.None,
                    1.5f * scale);

                MirageLayout.Cursor.Position = panelPos;
                using var panel = ImRaii.Child(
                    "##MirageMessagePanel"u8,
                    new Vector2(panelWidth, panelHeight),
                    false,
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                if (!panel)
                    return;

                MirageLayout.Cursor.Position = new Vector2(padding, padding);
                DrawTitleRow(icon, title, innerWidth, scale, settings);

                if (hasMessage)
                {
                    MirageLayout.Cursor.Y += SectionSpacing * scale;
                    MirageLayout.Cursor.X = padding;
                    DrawMessageBody(message, innerWidth);
                }

                if (hasButtons)
                {
                    MirageLayout.Cursor.Y += SectionSpacing * scale;
                    MirageLayout.Cursor.X = padding;
                    DrawButtons(ref visible, buttons, innerWidth, scale, settings);
                }
            }
        }

        private static float MeasureTitleHeight(FontAwesomeIcon? icon, string title, float innerWidth, float scale)
        {
            using (PushFont(FontSize.Large))
            {
                var line = ImGui.GetTextLineHeight();
                var iconWidth = 0f;
                if (icon != null)
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    var iconSize = ImGui.CalcTextSize(icon.Value.ToIconString());
                    ImGui.PopFont();
                    iconWidth = iconSize.X + 8f * scale;
                    line = Math.Max(line, iconSize.Y);
                }

                var textWidth = Math.Max(1f, innerWidth - iconWidth);
                var textSize = ImGui.CalcTextSize(string.IsNullOrEmpty(title) ? " " : title, false, textWidth);
                return Math.Max(line, textSize.Y);
            }
        }

        private static float MeasureMessageHeight(string message, float innerWidth)
        {
            if (string.IsNullOrEmpty(message))
                return 0f;

            using (PushFont(FontSize.Default))
                return ImGui.CalcTextSize(message, false, Math.Max(1f, innerWidth)).Y;
        }

        private static void DrawTitleRow(
            FontAwesomeIcon? icon,
            string title,
            float innerWidth,
            float scale,
            MirageColorSettings settings)
        {
            var startX = MirageLayout.Cursor.X;
            var titleColor = settings.GetColor(Color.Title);
            var gap = 8f * scale;

            using (PushFont(FontSize.Large))
            {
                var rowTop = MirageLayout.Cursor.ScreenPosition;
                var textStartX = startX;

                if (icon != null)
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    var iconText = icon.Value.ToIconString();
                    var iconSize = ImGui.CalcTextSize(iconText);
                    ImGui.PopFont();

                    var line = ImGui.GetTextLineHeight();
                    var iconPos = rowTop + new Vector2(0f, (Math.Max(line, iconSize.Y) - iconSize.Y) * 0.5f);
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.GetWindowDrawList().AddText(iconPos, ToUInt(settings.GetColor(Color.Accent)), iconText);
                    ImGui.PopFont();
                    textStartX += iconSize.X + gap;
                }

                MirageLayout.Cursor.X = textStartX;
                var wrapWidth = Math.Max(1f, startX + innerWidth - textStartX);
                ImGui.PushTextWrapPos(textStartX + wrapWidth);
                ImGui.PushStyleColor(ImGuiCol.Text, titleColor);
                ImGui.TextWrapped(string.IsNullOrEmpty(title) ? " " : title);
                ImGui.PopStyleColor();
                ImGui.PopTextWrapPos();
            }
        }

        private static void DrawMessageBody(string message, float innerWidth)
        {
            using (PushFont(FontSize.Default))
            {
                ImGui.PushTextWrapPos(MirageLayout.Cursor.X + Math.Max(1f, innerWidth));
                ImGui.PushStyleColor(ImGuiCol.Text, GetColor(Color.Default));
                ImGui.TextWrapped(message);
                ImGui.PopStyleColor();
                ImGui.PopTextWrapPos();
            }
        }

        private static void DrawButtons(
            ref bool visible,
            IList<MirageMessageButton> buttons,
            float innerWidth,
            float scale,
            MirageColorSettings settings)
        {
            var shown = buttons.Take(MaxButtons).ToList();
            if (shown.Count == 0)
                return;

            var gap = ButtonGap * scale;
            var totalGap = gap * (shown.Count - 1);
            var buttonWidth = Math.Max(ButtonMinWidth * scale, (innerWidth - totalGap) / shown.Count);
            var totalWidth = buttonWidth * shown.Count + totalGap;
            var startX = MirageLayout.Cursor.X + Math.Max(0f, (innerWidth - totalWidth) * 0.5f);
            var y = MirageLayout.Cursor.Y;

            for (var i = 0; i < shown.Count; i++)
            {
                var button = shown[i];
                MirageLayout.Cursor.Position = new Vector2(startX + i * (buttonWidth + gap), y);
                if (!DrawMessageButton(button, buttonWidth, settings))
                    continue;

                button.OnClick?.Invoke();
                if (button.CloseOnClick)
                    visible = false;
            }
        }

        private static bool DrawMessageButton(MirageMessageButton button, float width, MirageColorSettings settings)
        {
            var accent = settings.GetColor(Color.Accent);
            Vector4 bg;
            Vector4 hovered;
            Vector4 active;
            Vector4 border;
            Vector4 text;

            if (button.Primary)
            {
                bg = WithAlpha(accent, 0.28f);
                hovered = WithAlpha(accent, 0.42f);
                active = WithAlpha(accent, 0.55f);
                border = accent;
                text = settings.GetColor(Color.Default);
            }
            else
            {
                bg = WithAlpha(accent, 0.10f);
                hovered = WithAlpha(accent, 0.20f);
                active = WithAlpha(accent, 0.32f);
                border = WithAlpha(accent, 0.45f);
                text = settings.GetColor(Color.Secondary);
            }

            using var colors = ImRaii
                .PushColor(ImGuiCol.Button, bg)
                .Push(ImGuiCol.ButtonHovered, hovered)
                .Push(ImGuiCol.ButtonActive, active)
                .Push(ImGuiCol.Text, text)
                .Push(ImGuiCol.Border, border);
            using var styles = ImRaii
                .PushStyle(ImGuiStyleVar.FrameRounding, 4f)
                .Push(ImGuiStyleVar.FrameBorderSize, 1f);

            return ImGui.Button(button.Label, new Vector2(width, 0f));
        }
    }
}
