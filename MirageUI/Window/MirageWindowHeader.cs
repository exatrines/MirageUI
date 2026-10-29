using System.Runtime.InteropServices;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using MirageUI.Theme;
using MirageUI.Ui;

namespace MirageUI;

public static class MirageWindowHeader
{
    private const float BarHeightUnscaled = 36f;
    private const float ButtonSizeUnscaled = 22f;
    private const float ButtonGapUnscaled = 4f;
    private const float SidePaddingUnscaled = 12f;
    private const float CornerFraction = 0.22f;
    private const float PopupGapUnscaled = 8f;
    private const float PopupPadUnscaled = 12f;
    private const float PopupRowPadX = 8f;
    private const float PopupRowPadY = 6f;
    private const float PreviewLimitUnscaled = 320f;

    private static bool _leftWasDown;
    private static uint _backdropWindow;
    private static int _backdropFrame = -1;

    public const string PluginPageId = "plugin";

    public const string ThemePageId = "theme";

    public const string LanguagePageId = "language";

    public readonly record struct Section(string Id, string Label, FontAwesomeIcon Icon);

    public static float BarHeight => BarHeightUnscaled * ImGuiHelpers.GlobalScale;

    public static float RailWidth(
        bool collapsed,
        ReadOnlySpan<Section> sections,
        IReadOnlyList<MirageWindowFeature>? features = null)
    {
        var scale = ImGuiHelpers.GlobalScale;
        if (collapsed)
            return 16f * scale + RailButtonSize(scale);

        var pad = 8f * scale;
        var button = RailButtonSize(scale);
        var gap = 4f * scale;
        var label = LabelWidth(sections);
        var railFeatures = OrderedRail(features, collapsed: false);
        if (RailUsesLabels(railFeatures))
        {
            foreach (var feature in railFeatures)
            {
                if (!string.IsNullOrEmpty(feature.Label))
                    label = MathF.Max(label, ImGui.CalcTextSize(feature.Label).X);
            }

            return pad + button + label + 14f * scale + pad;
        }

        var count = 1 + railFeatures.Count;
        var utilities = pad + button * count + gap * Math.Max(0, count - 1) + pad;
        var sectionsRow = pad + button + label + 14f * scale + pad;
        return MathF.Max(sectionsRow, utilities);
    }

    public static void Draw(
        Window window,
        string title,
        ref bool collapsed,
        float leftInset,
        ref string pageId,
        IReadOnlyList<MirageWindowFeature>? features = null)
    {
        DrawBackdrop();
        var scale = ImGuiHelpers.GlobalScale;
        var height = BarHeight;
        var width = ImGui.GetWindowSize().X;
        var origin = ImGui.GetWindowPos();
        var button = ButtonSizeUnscaled * scale;
        var gap = ButtonGapUnscaled * scale;
        var side = SidePaddingUnscaled * scale;
        var titleFeatures = OrderedTitle(features);
        var customCount = titleFeatures.Count;
        var groupSplit = customCount > 0 ? gap + 1f : 0f;
        var buttonsWidth = button * (customCount + 3) + gap * (customCount + 2) + groupSplit;
        var buttonsX = width - side - buttonsWidth;
        var clickthrough = window.IsClickthrough;
        var leftDown = clickthrough && (GetKeyState(0x01) & 0x8000) != 0;
        var leftPressed = leftDown && !_leftWasDown;
        _leftWasDown = clickthrough && leftDown;

        var windowHeight = ImGui.GetWindowSize().Y;
        var corners = windowHeight <= height + 1f
            ? ImDrawFlags.RoundCornersAll
            : ImDrawFlags.RoundCornersTop;
        FillChrome(origin, origin + new Vector2(width, height), corners);

        ImGui.SetCursorPos(Vector2.Zero);
        ImGui.InvisibleButton("##HeaderDrag"u8, new Vector2(Math.Max(1f, buttonsX), height));
        if (!window.IsPinned
            && !clickthrough
            && ImGui.IsItemActive()
            && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            window.Position = ImGui.GetWindowPos() + ImGui.GetIO().MouseDelta;
            window.PositionCondition = ImGuiCond.Always;
        }

        var textSize = ImGui.CalcTextSize(title);
        var textPos = origin + new Vector2(leftInset + side, (height - textSize.Y) * 0.5f);
        var clipMax = origin.X + buttonsX - gap;
        if (textPos.X < clipMax)
        {
            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(origin, new Vector2(clipMax, origin.Y + height), true);
            drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), title);
            drawList.PopClipRect();
        }

        var buttonY = origin.Y + (height - button) * 0.5f;
        var x = origin.X + buttonsX;
        var popupGap = PopupGapUnscaled * scale;
        foreach (var feature in titleFeatures)
        {
            var popupId = $"##TitlePopup{feature.Id}";
            var min = new Vector2(x, buttonY);
            var opensPage = feature.TitleKind == MirageWindowFeatureKind.OpenPage
                && !string.IsNullOrEmpty(feature.PageId);
            var shortcut = feature.TitleKind == MirageWindowFeatureKind.Shortcut;
            if (IconButton($"##Title{feature.Id}", feature.Icon, min, button, clickthrough, leftPressed))
            {
                if (opensPage)
                {
                    pageId = feature.PageId!;
                    collapsed = false;
                }
                else if (shortcut)
                {
                    ImGui.OpenPopup(popupId);
                }
            }

            if (shortcut)
            {
                DrawFeaturePopup(
                    feature,
                    popupId,
                    new Vector2(min.X, buttonY + button + popupGap),
                    Vector2.Zero,
                    ref pageId,
                    ref collapsed,
                    feature.TitleOpensPage,
                    expandWindow: true);
            }

            x += button + gap;
        }

        if (customCount > 0)
        {
            var inset = 4f * scale;
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(x, buttonY + inset),
                new Vector2(x, buttonY + button - inset),
                ImGui.GetColorU32(ImGuiCol.Separator));
            x += 1f + gap;
        }

        var menuMin = new Vector2(x, buttonY);
        if (IconButton("##Menu", FontAwesomeIcon.Bars, menuMin, button, clickthrough, leftPressed))
            ImGui.OpenPopup("##WindowMenu");
        if (ImGui.IsPopupOpen("##WindowMenu"))
        {
            ImGui.SetNextWindowPos(new Vector2(menuMin.X, buttonY + button + popupGap), ImGuiCond.Appearing);
        }

        x += button + gap;
        var collapseIcon = collapsed ? FontAwesomeIcon.ChevronDown : FontAwesomeIcon.ChevronUp;
        if (IconButton("##Collapse", collapseIcon, new Vector2(x, buttonY), button, clickthrough, leftPressed))
            collapsed = !collapsed;

        x += button + gap;
        if (IconButton("##Close", FontAwesomeIcon.Times, new Vector2(x, buttonY), button, clickthrough, leftPressed))
            window.IsOpen = false;

        DrawMenu(window);

        var lineY = origin.Y + height - 1f;
        var splitX = origin.X + leftInset;
        ImGui.GetWindowDrawList().AddLine(
            new Vector2(splitX, lineY),
            new Vector2(origin.X + width, lineY),
            ImGui.GetColorU32(ImGuiCol.Separator));

        ImGui.SetCursorPos(new Vector2(leftInset, height));
    }

    public static void DrawBackdrop()
    {
        if (ImGui.GetWindowViewport().ID != ImGui.GetMainViewport().ID)
            return;

        var window = ImGuiP.GetCurrentWindow();
        var frame = ImGui.GetFrameCount();
        if (window.ID == _backdropWindow && frame == _backdropFrame)
            return;

        _backdropWindow = window.ID;
        _backdropFrame = frame;

        var strength = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).BlurStrength;
        if (strength <= 0f)
            return;

        var min = ImGui.GetWindowPos();
        var max = min + ImGui.GetWindowSize();
        ImGuiHelpers.PrependBlurBehind(
            ImGui.GetWindowDrawList(),
            min,
            max,
            blurStrength: strength,
            rounding: ImGui.GetStyle().WindowRounding,
            noiseOpacity: 0f);
    }

    public static void FillContentPage(ImDrawFlags corners = ImDrawFlags.RoundCornersBottomRight)
    {
        var min = ImGui.GetCursorScreenPos();
        var max = min + ImGui.GetContentRegionAvail();
        var coat = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).PageBg;
        ImGui.GetWindowDrawList().AddRectFilled(
            min,
            max,
            ImGui.GetColorU32(coat),
            ImGui.GetStyle().WindowRounding,
            corners);
    }

    public static void DrawRailDivider(float railWidth)
    {
        var origin = ImGui.GetWindowPos();
        var x = origin.X + railWidth;
        ImGui.GetWindowDrawList().AddLine(
            new Vector2(x, origin.Y + BarHeight),
            new Vector2(x, origin.Y + ImGui.GetWindowSize().Y),
            ImGui.GetColorU32(ImGuiCol.Separator));
    }

    private static void FillChrome(Vector2 min, Vector2 max, ImDrawFlags corners)
    {
        var coat = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).ChromeBg;
        ImGui.GetWindowDrawList().AddRectFilled(
            min,
            max,
            ImGui.GetColorU32(coat),
            ImGui.GetStyle().WindowRounding,
            corners);
    }

    public static void DrawRail(
        ReadOnlySpan<Section> sections,
        ref string selectedId,
        ref bool collapsed,
        bool showSections,
        IReadOnlyList<MirageWindowFeature>? features = null)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var rail = RailWidth(collapsed, sections, features);
        var height = BarHeight;
        var origin = ImGui.GetWindowPos();
        var windowHeight = ImGui.GetWindowSize().Y;

        if (!showSections)
            return;

        FillChrome(
            new Vector2(origin.X, origin.Y + height),
            new Vector2(origin.X + rail, origin.Y + windowHeight),
            ImDrawFlags.RoundCornersBottomLeft);

        var pad = 8f * scale;
        var button = RailButtonSize(scale);
        var gap = 4f * scale;
        var inset = pad;
        var cursorY = height + pad + DrawMark(origin, rail, height + pad, scale, collapsed, selectedId == PluginPageId, ref selectedId);
        cursorY += pad;
        DrawRailSeparator(origin, rail, cursorY, inset);
        cursorY += pad;
        foreach (var section in sections)
        {
            var selected = selectedId == section.Id;
            var bb = SectionBounds(origin, rail, cursorY, scale, collapsed);
            if (SectionButton(section.Id, bb, out var hovered) && !selected)
                selectedId = section.Id;

            DrawSection(section, bb, selected, hovered, collapsed, scale);
            if (collapsed && hovered)
                MirageTooltip.Show(section.Label, MirageTooltipPosition.Right, bb);

            cursorY += bb.GetHeight() + 4f * scale;
        }

        var railFeatures = OrderedRail(features, collapsed);
        if (collapsed)
        {
            var rows = railFeatures.Count + 1;
            var block = button * rows + gap * Math.Max(0, rows - 1);
            var y = windowHeight - pad - block;
            DrawRailSeparator(origin, rail, y - pad, inset);
            foreach (var feature in railFeatures)
            {
                var min = new Vector2(origin.X + (rail - button) * 0.5f, origin.Y + y);
                DrawRailFeature(
                    feature,
                    new ImRect(min, min + new Vector2(button, button)),
                    iconOnly: true,
                    MirageTooltipPosition.Right,
                    ref selectedId,
                    scale);
                y += button + gap;
            }

            DrawRailToggle(
                new Vector2(origin.X + (rail - button) * 0.5f, origin.Y + y),
                button,
                expand: true,
                MirageTooltipPosition.Right,
                ref collapsed,
                scale);
            return;
        }

        if (RailUsesLabels(railFeatures))
        {
            var rows = railFeatures.Count + 1;
            var block = button * rows + gap * Math.Max(0, rows - 1);
            var y = windowHeight - pad - block;
            DrawRailSeparator(origin, rail, y - pad, inset);
            foreach (var feature in railFeatures)
            {
                DrawRailFeature(
                    feature,
                    SectionBounds(origin, rail, y, scale, collapsed: false),
                    iconOnly: false,
                    MirageTooltipPosition.Above,
                    ref selectedId,
                    scale);
                y += button + gap;
            }

            DrawRailToggle(
                new Vector2(origin.X + pad, origin.Y + y),
                button,
                expand: false,
                MirageTooltipPosition.Above,
                ref collapsed,
                scale);
            return;
        }

        var rowTop = windowHeight - pad - button;
        DrawRailSeparator(origin, rail, rowTop - pad, inset);
        var x = origin.X + pad;
        DrawRailToggle(
            new Vector2(x, origin.Y + rowTop),
            button,
            expand: false,
            MirageTooltipPosition.Above,
            ref collapsed,
            scale);
        x += button + gap;
        foreach (var feature in railFeatures)
        {
            var min = new Vector2(x, origin.Y + rowTop);
            DrawRailFeature(
                feature,
                new ImRect(min, min + new Vector2(button, button)),
                iconOnly: true,
                MirageTooltipPosition.Above,
                ref selectedId,
                scale);
            x += button + gap;
        }
    }

    private static float RailButtonSize(float scale) =>
        MathF.Max(ImGui.GetTextLineHeight(), 26f * scale) + 12f * scale;

    private static void DrawRailSeparator(Vector2 origin, float rail, float y, float inset)
    {
        var line = ImGui.GetColorU32(ImGuiCol.Separator);
        ImGui.GetWindowDrawList().AddLine(
            new Vector2(origin.X + inset, origin.Y + y),
            new Vector2(origin.X + rail - inset, origin.Y + y),
            line);
    }

    private static ImRect SectionBounds(Vector2 origin, float rail, float cursorY, float scale, bool collapsed)
    {
        var size = RailButtonSize(scale);
        if (collapsed)
        {
            var min = origin + new Vector2((rail - size) * 0.5f, cursorY);
            return new ImRect(min, min + new Vector2(size, size));
        }

        var pad = 8f * scale;
        var rowMin = origin + new Vector2(pad, cursorY);
        return new ImRect(rowMin, rowMin + new Vector2(rail - pad * 2f, size));
    }

    private static float LabelWidth(ReadOnlySpan<Section> sections)
    {
        var width = 0f;
        foreach (var section in sections)
            width = MathF.Max(width, ImGui.CalcTextSize(section.Label).X);
        return width;
    }

    private static void DrawSection(
        Section section,
        ImRect bb,
        bool selected,
        bool hovered,
        bool collapsed,
        float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        if (hovered || selected)
        {
            var col = ImGui.GetColorU32(hovered ? ImGuiCol.HeaderHovered : ImGuiCol.Header);
            drawList.AddRectFilled(bb.Min, bb.Max, col, CornerRadius(bb));
        }

        var color = selected
            ? MirageUi.ToUInt(MirageUi.GetColor(MirageUi.Color.Accent))
            : ImGui.GetColorU32(ImGuiCol.Text);
        var slot = RailButtonSize(scale);
        var glyph = section.Icon.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var glyphSize = ImGui.CalcTextSize(glyph);
        var glyphPos = new Vector2(
            bb.Min.X + (slot - glyphSize.X) * 0.5f,
            bb.Min.Y + (bb.GetHeight() - glyphSize.Y) * 0.5f);
        drawList.AddText(glyphPos, color, glyph);
        ImGui.PopFont();

        if (collapsed)
            return;

        var text = ImGui.CalcTextSize(section.Label);
        drawList.AddText(
            new Vector2(bb.Min.X + slot, bb.Min.Y + (bb.GetHeight() - text.Y) * 0.5f),
            color,
            section.Label);
    }

    private static float DrawMark(
        Vector2 origin,
        float rail,
        float top,
        float scale,
        bool collapsed,
        bool selected,
        ref string selectedId)
    {
        var icon = MathF.Round(collapsed ? RailButtonSize(scale) : 48f * scale);
        var raw = origin + new Vector2((rail - icon) * 0.5f, top);
        var pos = new Vector2(MathF.Round(raw.X), MathF.Round(raw.Y));
        var size = new Vector2(icon, icon);
        var bb = new ImRect(pos, pos + size);
        var drawList = ImGui.GetWindowDrawList();
        var hovered = bb.Contains(ImGui.GetMousePos());
        var radius = IconCornerRadius(size);
        DrawIconShadow(drawList, pos, size, radius, scale, hovered);
        if (hovered)
        {
            var pad = 1f * scale;
            drawList.AddRectFilled(
                pos - new Vector2(pad, pad),
                pos + size + new Vector2(pad, pad),
                ImGui.GetColorU32(ImGuiCol.HeaderHovered),
                radius <= 0f ? 0f : radius + pad);
        }

        var path = MirageUi.PluginInfo.IconPath;
        if (string.IsNullOrWhiteSpace(path) || !ImageRegistry.TryGetWrap(path, out var texture))
        {
            var glyph = MirageUi.PluginInfo.TitleBarIcon.ToIconString();
            ImGui.PushFont(UiBuilder.IconFont);
            var glyphSize = ImGui.CalcTextSize(glyph);
            drawList.AddText(pos + (size - glyphSize) * 0.5f, ImGui.GetColorU32(ImGuiCol.Text), glyph);
            ImGui.PopFont();
        }
        else
        {
            drawList.AddImage(texture.Handle, pos, pos + size);
        }

        ImGui.SetCursorScreenPos(pos);
        if (ImGui.InvisibleButton("##PluginMark"u8, size) && !selected)
            selectedId = PluginPageId;

        return icon;
    }

    private static float IconCornerRadius(Vector2 size)
    {
        var fraction = MathF.Max(0f, MirageUi.PluginInfo.IconCornerFraction);
        return MathF.Min(size.X, size.Y) * fraction;
    }

    private static void DrawIconShadow(
        ImDrawListPtr drawList,
        Vector2 pos,
        Vector2 size,
        float radius,
        float scale,
        bool hovered)
    {
        var spread = (hovered ? 7f : 5f) * scale;
        var drop = new Vector2(0f, (hovered ? 2f : 1.5f) * scale);
        var strength = hovered ? 0.16f : 0.09f;
        const int steps = 4;
        for (var i = steps; i >= 1; i--)
        {
            var t = i / (float)steps;
            var alpha = strength * (1f - t);
            if (alpha <= 0.012f)
                continue;

            var grow = spread * t;
            var min = pos + drop - new Vector2(grow, grow);
            var max = pos + size + drop + new Vector2(grow, grow);
            var rounding = radius <= 0f ? 0f : radius + grow;
            drawList.AddRectFilled(
                min,
                max,
                ImGui.GetColorU32(new Vector4(0f, 0f, 0f, alpha)),
                rounding);
        }
    }

    private static bool SectionButton(string id, ImRect bb, out bool hovered)
    {
        hovered = false;
        var window = ImGuiP.GetCurrentWindow();
        var widgetId = window.GetID(id);
        ImGui.SetCursorScreenPos(bb.Min);
        ImGuiP.ItemSize(bb.GetSize());
        if (!ImGuiP.ItemAdd(bb, widgetId))
            return false;

        var held = false;
        var pressed = ImGuiP.ButtonBehavior(bb, widgetId, ref hovered, ref held, ImGuiButtonFlags.None);
        if (pressed)
            ImGuiP.MarkItemEdited(widgetId);

        return pressed;
    }

    private static string Text(string key) => MirageUi.I18n.Get(key);

    private static void DrawMenu(Window window)
    {
        var pin = Text("window.pin");
        var pinInfo = Text("window.pin.info");
        var clickthroughLabel = Text("window.clickthrough");
        var clickthroughInfo = Text("window.clickthrough.info");
        var copyScreenshot = Text("window.screenshot");
        var preview = Text("window.preview");
        var contentWidth = MenuContentWidth(pin, clickthroughLabel, copyScreenshot);
        var menuOpen = ImGui.IsPopupOpen("##WindowMenu");
        if (menuOpen)
        {
            var pad = PopupPadUnscaled * ImGuiHelpers.GlobalScale;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(pad, pad));
            var width = contentWidth + pad * 2f;
            ImGui.SetNextWindowSizeConstraints(new Vector2(width, 0f), new Vector2(width, float.MaxValue));
        }

        if (!ImGui.BeginPopup("##WindowMenu"))
        {
            if (menuOpen)
                ImGui.PopStyleVar();
            MirageWindowCapture.Retire();
            return;
        }

        var pinned = window.IsPinned;
        if (MirageUi.Checkbox(pin, ref pinned))
            window.IsPinned = pinned;
        MenuInfo("##MenuPinInfo", pinInfo);

        ImGui.SameLine();
        var clickthrough = window.IsClickthrough;
        if (MirageUi.Checkbox(clickthroughLabel, ref clickthrough))
            window.IsClickthrough = clickthrough;
        MenuInfo("##MenuClickthroughInfo", clickthroughInfo);

        ImGui.Separator();

        var shot = MirageWindowCapture.Preview;
        if (shot != null)
            ImGui.Image(shot.Handle, PreviewSize(shot.Size, contentWidth));

        var side = MirageUi.ResolveControlHeight();
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var copyWidth = MathF.Max(1f, contentWidth - spacing - side);
        if (MirageUi.PrimaryButton(copyScreenshot, width: copyWidth, id: "copyScreenshot"))
        {
            if (MirageWindowCapture.Preview != null)
                MirageWindowCapture.CopyHeld();
            else
                MirageWindowCapture.Request(window.WindowName, copy: true);
        }

        ImGui.SameLine(0f, spacing);
        if (MirageUi.IconButton(
                FontAwesomeIcon.Eye,
                id: "windowPreview",
                size: new Vector2(side, side),
                tooltip: preview))
            MirageWindowCapture.Request(window.WindowName, preview: true);

        ImGui.EndPopup();
        ImGui.PopStyleVar();
    }

    private static float MenuContentWidth(string pin, string clickthrough, string copyScreenshot)
    {
        var style = ImGui.GetStyle();
        var frame = ImGui.GetFrameHeight();
        var info = frame;
        var checks = frame + style.ItemInnerSpacing.X + ImGui.CalcTextSize(pin).X + style.ItemInnerSpacing.X + info
            + style.ItemSpacing.X
            + frame + style.ItemInnerSpacing.X + ImGui.CalcTextSize(clickthrough).X + style.ItemInnerSpacing.X + info;
        var pad = MirageUi.ResolveInputFramePadding();
        var copy = ImGui.CalcTextSize(copyScreenshot).X + (pad.X + 6f) * 2f
            + style.ItemSpacing.X + MirageUi.ResolveControlHeight();
        return MathF.Max(PreviewLimitUnscaled * ImGuiHelpers.GlobalScale, MathF.Max(checks, copy));
    }

    private static void MenuInfo(string id, string tooltip)
    {
        var frame = ImGui.GetFrameHeight();
        ImGui.SameLine(0f, ImGui.GetStyle().ItemInnerSpacing.X);
        MirageUi.IconButton(
            FontAwesomeIcon.InfoCircle,
            id: id,
            size: new Vector2(frame, frame),
            color: MirageUi.GetColor(MirageUi.Color.Secondary),
            tooltip: tooltip);
    }

    private static Vector2 PreviewSize(Vector2 size, float limit)
    {
        if (size.X < 1f || size.Y < 1f)
            return size;

        var scale = 1f;
        if (size.X > limit)
            scale = limit / size.X;
        if (size.Y * scale > limit)
            scale = limit / size.Y;
        return size * scale;
    }

    private static bool IconButton(
        string id,
        FontAwesomeIcon icon,
        Vector2 screenMin,
        float size,
        bool clickthrough,
        bool leftPressed,
        string? tooltip = null)
    {
        ImGui.SetCursorScreenPos(screenMin);
        var bb = new ImRect(screenMin, screenMin + new Vector2(size, size));
        var hovered = bb.Contains(ImGui.GetMousePos());
        var pressed = false;

        if (!clickthrough)
        {
            var window = ImGuiP.GetCurrentWindow();
            var widgetId = window.GetID(id);
            ImGuiP.ItemSize(bb.GetSize());
            if (!ImGuiP.ItemAdd(bb, widgetId))
                return false;

            var held = false;
            pressed = ImGuiP.ButtonBehavior(bb, widgetId, ref hovered, ref held, ImGuiButtonFlags.None);
            if (pressed)
                ImGuiP.MarkItemEdited(widgetId);
        }
        else if (hovered && leftPressed)
        {
            pressed = true;
        }

        if (hovered && !string.IsNullOrEmpty(tooltip))
            MirageTooltip.Show(tooltip, MirageTooltipPosition.Below, bb);

        var iconText = icon.ToIconString();
        ImGui.PushFont(UiBuilder.IconFont);
        var iconSize = ImGui.CalcTextSize(iconText);
        var color = hovered
            ? MirageUi.ToUInt(MirageUi.GetColor(MirageUi.Color.Accent))
            : ImGui.GetColorU32(ImGuiCol.Text);
        ImGui.GetWindowDrawList().AddText(
            screenMin + (new Vector2(size, size) - iconSize) * 0.5f,
            color,
            iconText);
        ImGui.PopFont();

        return pressed;
    }

    private static float CornerRadius(ImRect bb) =>
        MathF.Min(bb.GetWidth(), bb.GetHeight()) * CornerFraction;

    public static bool PopupRow(string label, bool selected, string id)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var pad = new Vector2(PopupRowPadX, PopupRowPadY) * scale;
        var textSize = ImGui.CalcTextSize(label);
        var width = ImGui.GetContentRegionAvail().X;
        var height = textSize.Y + pad.Y * 2f;
        var clicked = ImGui.Selectable($"##{id}", selected, ImGuiSelectableFlags.None, new Vector2(width, height));
        var min = ImGui.GetItemRectMin();
        ImGui.GetWindowDrawList().AddText(
            new Vector2(min.X + pad.X, min.Y + pad.Y),
            ImGui.GetColorU32(ImGuiCol.Text),
            label);
        if (selected)
            ImGui.SetItemDefaultFocus();
        return clicked;
    }

    private static List<MirageWindowFeature> OrderedTitle(IReadOnlyList<MirageWindowFeature>? features)
    {
        if (features == null || features.Count == 0)
            return [];

        return features
            .Select((feature, index) => (feature, index))
            .Where(item => item.feature.ShowOnTitle)
            .OrderBy(item => item.feature.TitleOrder)
            .ThenBy(item => item.index)
            .Select(item => item.feature)
            .ToList();
    }

    private static List<MirageWindowFeature> OrderedRail(IReadOnlyList<MirageWindowFeature>? features, bool collapsed)
    {
        if (features == null || features.Count == 0)
            return [];

        return features
            .Select((feature, index) => (feature, index))
            .Where(item => ShowsOnRail(item.feature, collapsed))
            .OrderBy(item => item.feature.RailOrder)
            .ThenBy(item => item.index)
            .Select(item => item.feature)
            .ToList();
    }

    private static bool ShowsOnRail(MirageWindowFeature feature, bool collapsed)
    {
        if (!feature.ShowOnRail)
            return false;

        return feature.RailWhen == MirageWindowRailWhen.Both
            || (collapsed && feature.RailWhen == MirageWindowRailWhen.Collapsed)
            || (!collapsed && feature.RailWhen == MirageWindowRailWhen.Expanded);
    }

    private static bool RailUsesLabels(List<MirageWindowFeature> features) =>
        features.Exists(feature => !string.IsNullOrEmpty(feature.Label));

    private static void DrawRailToggle(
        Vector2 min,
        float button,
        bool expand,
        MirageTooltipPosition tooltipPosition,
        ref bool collapsed,
        float scale)
    {
        var bounds = new ImRect(min, min + new Vector2(button, button));
        if (SectionButton("##RailCollapse", bounds, out var hovered))
            collapsed = !expand;

        var key = expand ? "window.expand" : "window.collapse";
        var icon = expand ? FontAwesomeIcon.AngleDoubleRight : FontAwesomeIcon.AngleDoubleLeft;
        DrawSection(new Section("collapse", Text(key), icon), bounds, selected: false, hovered, collapsed: true, scale);
        if (hovered)
            MirageTooltip.Show(Text(key), tooltipPosition, bounds);
    }

    private static void DrawRailFeature(
        MirageWindowFeature feature,
        ImRect bounds,
        bool iconOnly,
        MirageTooltipPosition tooltipPosition,
        ref string selectedId,
        float scale)
    {
        var popupId = $"##RailPopup{feature.Id}";
        var opensPage = feature.RailKind == MirageWindowFeatureKind.OpenPage
            && !string.IsNullOrEmpty(feature.PageId);
        var shortcut = feature.RailKind == MirageWindowFeatureKind.Shortcut;
        var selected = opensPage && selectedId == feature.PageId;
        if (SectionButton($"##Rail{feature.Id}", bounds, out var hovered))
        {
            if (opensPage && !selected)
                selectedId = feature.PageId!;
            else if (shortcut)
                ImGui.OpenPopup(popupId);
        }

        DrawSection(
            new Section(feature.Id, feature.Label ?? string.Empty, feature.Icon),
            bounds,
            selected,
            hovered,
            iconOnly,
            scale);
        if (hovered && iconOnly)
        {
            var tip = string.IsNullOrEmpty(feature.Tooltip) ? feature.Label : feature.Tooltip;
            if (!string.IsNullOrEmpty(tip))
                MirageTooltip.Show(tip, tooltipPosition, bounds);
        }

        if (!shortcut)
            return;

        var popupGap = PopupGapUnscaled * scale;
        var pos = tooltipPosition == MirageTooltipPosition.Right
            ? new Vector2(bounds.Max.X + popupGap, bounds.Max.Y)
            : new Vector2(bounds.Min.X, bounds.Min.Y - popupGap);
        var ignored = false;
        DrawFeaturePopup(
            feature,
            popupId,
            pos,
            new Vector2(0f, 1f),
            ref selectedId,
            ref ignored,
            feature.RailOpensPage,
            expandWindow: false);
    }

    private static void DrawFeaturePopup(
        MirageWindowFeature feature,
        string popupId,
        Vector2 pos,
        Vector2 pivot,
        ref string pageId,
        ref bool windowCollapsed,
        bool openPage,
        bool expandWindow)
    {
        if (!ImGui.IsPopupOpen(popupId))
            return;

        var showFooter = openPage && !string.IsNullOrEmpty(feature.PageId);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Appearing, pivot);
        var width = PopupWidth(feature, showFooter);
        ImGui.SetNextWindowSizeConstraints(new Vector2(width, 0f), new Vector2(width, float.MaxValue));
        var scale = ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(PopupPadUnscaled, PopupPadUnscaled) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 4f * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
        if (!ImGui.BeginPopup(popupId))
        {
            ImGui.PopStyleVar(3);
            return;
        }

        feature.DrawPopup?.Invoke(popupId);
        if (showFooter)
        {
            ImGui.Separator();
            var label = string.IsNullOrEmpty(feature.OpenPageLabel) ? feature.PageId! : feature.OpenPageLabel;
            if (PopupRow(label, false, $"open-{feature.Id}"))
            {
                pageId = feature.PageId!;
                if (expandWindow)
                    windowCollapsed = false;
            }
        }

        ImGui.EndPopup();
        ImGui.PopStyleVar(3);
    }

    private static float PopupWidth(MirageWindowFeature feature, bool showFooter)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var text = feature.PopupTextWidth?.Invoke() ?? 0f;
        if (showFooter)
        {
            var footer = string.IsNullOrEmpty(feature.OpenPageLabel) ? feature.PageId ?? string.Empty : feature.OpenPageLabel;
            text = MathF.Max(text, ImGui.CalcTextSize(footer).X);
        }

        return text + (PopupPadUnscaled * 2f + PopupRowPadX * 2f) * scale + 2f;
    }

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);
}
