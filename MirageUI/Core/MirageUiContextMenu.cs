using Dalamud.Interface;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>Styled popup context menu (ported from EnhancedQuickPanel).</summary>
    public static class ContextMenu
    {
        private const float IconTextGap = 6f;

        private const ImGuiWindowFlags PopupFlags =
            ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoSavedSettings;

        /// <summary>Applies context-menu chrome within a using scope.</summary>
        public readonly struct StyleScope : IDisposable
        {
            private readonly ImRaii.ColorDisposable _popupBg;
            private readonly ImRaii.ColorDisposable _windowBg;
            private readonly ImRaii.ColorDisposable _childBg;
            private readonly ImRaii.StyleDisposable _windowPadding;
            private readonly ImRaii.StyleDisposable _itemSpacing;
            private readonly ImRaii.StyleDisposable _windowBorderSize;

            public StyleScope(MirageContextMenuStyle style)
            {
                style.EnsureDefaults();
                var outer = style.OuterPadding;
                var bg = style.BgColor;
                _popupBg = ImRaii.PushColor(ImGuiCol.PopupBg, bg);
                _windowBg = ImRaii.PushColor(ImGuiCol.WindowBg, bg);
                _childBg = ImRaii.PushColor(ImGuiCol.ChildBg, bg);
                _windowPadding = ImRaii.PushStyle(
                    ImGuiStyleVar.WindowPadding,
                    new Vector2(outer, outer));
                _itemSpacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
                _windowBorderSize = ImRaii.PushStyle(ImGuiStyleVar.WindowBorderSize, 0f);
            }

            public void Dispose()
            {
                _windowBorderSize.Dispose();
                _itemSpacing.Dispose();
                _windowPadding.Dispose();
                _childBg.Dispose();
                _windowBg.Dispose();
                _popupBg.Dispose();
            }
        }

        public static float ComputeRequiredWidth(MirageContextMenuStyle style, ReadOnlySpan<string> labels)
        {
            style.EnsureDefaults();
            var padding = style.Padding;
            var outer = style.OuterPadding;
            var maxText = 0f;
            foreach (var label in labels)
                maxText = Math.Max(maxText, ImGui.CalcTextSize(label).X);

            var checkWidth = ImGui.CalcTextSize("✓").X;
            Vector2 iconSize;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                iconSize = ImGui.CalcTextSize(FontAwesomeIcon.Clipboard.ToIconString());

            var contentWidth = padding * 2f + iconSize.X + IconTextGap + maxText + checkWidth;
            var minWidth = contentWidth + outer * 2f;
            return Math.Max(style.Width + outer * 2f, minWidth);
        }

        public static float ComputeRequiredWidth(MirageContextMenuStyle style, params string[] labels) =>
            ComputeRequiredWidth(style, labels.AsSpan());

        public static float ComputeRowHeight(MirageContextMenuStyle style, ReadOnlySpan<string> labels)
        {
            style.EnsureDefaults();
            var padding = style.Padding;
            var textHeight = 0f;
            foreach (var label in labels)
                textHeight = Math.Max(textHeight, ImGui.CalcTextSize(label).Y);

            textHeight = Math.Max(textHeight, ImGui.CalcTextSize("✓").Y);
            Vector2 iconSize;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                iconSize = ImGui.CalcTextSize(FontAwesomeIcon.Clipboard.ToIconString());

            return Math.Max(textHeight, iconSize.Y) + padding * 2f;
        }

        public static float ComputeRowHeight(MirageContextMenuStyle style, params string[] labels) =>
            ComputeRowHeight(style, labels.AsSpan());

        /// <summary>
        /// Sets popup size for the given labels, then begins the popup with EQP-style chrome.
        /// Call <see cref="End"/> when this returns true.
        /// </summary>
        public static bool Begin(
            string id,
            ReadOnlySpan<string> labels,
            MirageContextMenuStyle? style = null)
        {
            style ??= MirageContextMenuStyle.CreateDefault();
            style.EnsureDefaults();

            if (labels.Length == 0)
                return false;

            var menuWidth = ComputeRequiredWidth(style, labels);
            var rowHeight = ComputeRowHeight(style, labels);
            var windowHeight = rowHeight * labels.Length + style.OuterPadding * 2f;
            ImGui.SetNextWindowSize(new Vector2(menuWidth, windowHeight), ImGuiCond.Always);

            // Style must stay pushed while the popup is open; End() pops via StyleStack.
            StyleStack.Push(new StyleScope(style));
            if (!ImGui.BeginPopup(id, PopupFlags))
            {
                StyleStack.Pop().Dispose();
                return false;
            }

            return true;
        }

        public static bool Begin(
            string id,
            IReadOnlyList<string> labels,
            MirageContextMenuStyle? style = null)
        {
            var array = new string[labels.Count];
            for (var i = 0; i < labels.Count; i++)
                array[i] = labels[i];
            return Begin(id, array.AsSpan(), style);
        }

        public static void End()
        {
            ImGui.EndPopup();
            if (StyleStack.Count > 0)
                StyleStack.Pop().Dispose();
        }

        /// <summary>Draw one context-menu row. Returns true when clicked.</summary>
        public static bool DrawItem(
            string label,
            FontAwesomeIcon icon,
            string id,
            MirageContextMenuStyle? style = null,
            string? trailingLabel = null,
            FontAwesomeIcon? trailingIcon = null,
            float indent = 0f)
        {
            style ??= MirageContextMenuStyle.CreateDefault();
            style.EnsureDefaults();
            if (indent > 0f)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + indent);

            var padding = new Vector2(style.Padding, style.Padding);
            var textSize = ImGui.CalcTextSize(label);
            var trailingText = trailingLabel ?? string.Empty;
            var trailingIconText = trailingIcon?.ToIconString() ?? string.Empty;
            Vector2 trailingSize;
            if (trailingIcon != null)
            {
                using (ImRaii.PushFont(UiBuilder.IconFont))
                    trailingSize = ImGui.CalcTextSize(trailingIconText);
            }
            else if (!string.IsNullOrEmpty(trailingText))
            {
                trailingSize = ImGui.CalcTextSize(trailingText);
            }
            else
            {
                trailingSize = Vector2.Zero;
            }

            var iconText = icon.ToIconString();
            Vector2 iconSize;
            using (ImRaii.PushFont(UiBuilder.IconFont))
                iconSize = ImGui.CalcTextSize(iconText);

            var contentHeight = Math.Max(textSize.Y, iconSize.Y);
            if (trailingSize != Vector2.Zero)
                contentHeight = Math.Max(contentHeight, trailingSize.Y);

            var itemWidth = ImGui.GetContentRegionAvail().X;
            if (itemWidth <= 0f)
                itemWidth = Math.Max(0f, style.Width - style.Padding * 2f);
            var itemHeight = contentHeight + padding.Y * 2f;

            using (ImRaii.PushId(id))
            using (ImRaii.PushStyle(ImGuiStyleVar.FramePadding, padding))
            using (ImRaii.PushColor(ImGuiCol.Header, style.ButtonBgColor))
            using (ImRaii.PushColor(ImGuiCol.HeaderHovered, style.ButtonBgHoverColor))
            using (ImRaii.PushColor(ImGuiCol.HeaderActive, style.ButtonBgHoverColor))
            {
                var clicked = ImGui.Selectable(
                    "##MirageContextMenuItem",
                    false,
                    ImGuiSelectableFlags.None,
                    new Vector2(itemWidth, itemHeight));

                var hovered = ImGui.IsItemHovered();
                var textColor = hovered ? style.TextHoverColor : style.TextColor;
                var color = ImGui.ColorConvertFloat4ToU32(textColor);
                var rectMin = ImGui.GetItemRectMin();
                var rectMax = ImGui.GetItemRectMax();
                var rowHeight = rectMax.Y - rectMin.Y;
                var iconPos = new Vector2(
                    rectMin.X + padding.X,
                    rectMin.Y + (rowHeight - iconSize.Y) * 0.5f);
                var textPos = new Vector2(
                    iconPos.X + iconSize.X + IconTextGap,
                    rectMin.Y + (rowHeight - textSize.Y) * 0.5f);

                var drawList = ImGui.GetWindowDrawList();
                using (ImRaii.PushFont(UiBuilder.IconFont))
                    drawList.AddText(iconPos, color, iconText);
                drawList.AddText(textPos, color, label);

                if (trailingSize != Vector2.Zero)
                {
                    var trailingPos = new Vector2(
                        rectMax.X - padding.X - trailingSize.X,
                        rectMin.Y + (rowHeight - trailingSize.Y) * 0.5f);
                    if (trailingIcon != null)
                    {
                        using (ImRaii.PushFont(UiBuilder.IconFont))
                            drawList.AddText(trailingPos, color, trailingIconText);
                    }
                    else
                    {
                        drawList.AddText(trailingPos, color, trailingText);
                    }
                }

                return clicked;
            }
        }

        private static readonly Stack<StyleScope> StyleStack = new();
    }
}
