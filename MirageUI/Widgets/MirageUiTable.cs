using MirageUI.Theme;

namespace MirageUI;

public enum MirageTableAlign
{
    Left,
    Center,
}

public sealed class MirageTableColumn
{
    public string Label { get; init; } = string.Empty;

    public float Width { get; init; } = 80f;

    public bool Stretch { get; init; }

    public MirageTableAlign Align { get; init; } = MirageTableAlign.Left;
}

public static partial class MirageUi
{
    // Fixed header, scrolling rows, fixed add row. Horizontal scroll moves all three together.
    public static class Table
    {
        private const ImGuiTableFlags TableFlags =
            ImGuiTableFlags.SizingFixedFit
            | ImGuiTableFlags.NoPadOuterX
            | ImGuiTableFlags.PreciseWidths;

        private static MirageTableColumn[] _columns = [];
        private static int _column;
        private static int _alignVars;
        private static float _cellY;

        public static float Width { get; private set; }

        public static float VisibleWidth { get; private set; }

        public static float ScrollX { get; private set; }

        public static Vector2 CellOrigin { get; private set; }

        public static bool NextColumn()
        {
            PopAlign();
            var entered = ImGui.TableNextColumn();
            CellOrigin = ImGui.GetCursorScreenPos();
            _cellY = ImGui.GetCursorPosY();
            var column = _column < _columns.Length ? _columns[_column] : null;
            _column++;
            var align = column?.Align ?? MirageTableAlign.Left;
            if (align == MirageTableAlign.Center && column is { Stretch: false } && column.Width <= ResolveControlHeight() * 2f)
            {
                var item = ResolveControlHeight();
                var avail = ImGui.GetContentRegionAvail().X;
                var shift = MathF.Max(0f, (avail - item) * 0.5f);
                if (shift > 0.5f)
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + shift);
            }

            var x = align == MirageTableAlign.Center ? 0.5f : 0f;
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(x, 0.5f));
            _alignVars++;
            return entered;
        }

        public static void CenterItem(float itemHeight)
        {
            var shift = MathF.Max(0f, (ResolveControlHeight() - itemHeight) * 0.5f);
            ImGui.SetCursorPosY(_cellY + shift);
        }

        public static void Draw(
            string id,
            IReadOnlyList<MirageTableColumn> columns,
            int rowCount,
            Func<int, bool> drawRow,
            Action? drawAddRow = null)
        {
            if (columns.Count == 0)
                return;

            var style = ImGui.GetStyle();
            var scale = MathF.Max(0.01f, MirageLayout.Style.Scale);
            var rounding = InputStyle.FrameRounding * scale;
            var inset = rounding;
            var vBar = style.ScrollbarSize;
            var hostSize = ImGui.GetContentRegionAvail();
            if (hostSize.X < 1f || hostSize.Y < 1f)
                return;

            using var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            using var host = ImRaii.Child(
                id,
                hostSize,
                false,
                ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoSavedSettings);
            if (!host)
                return;

            var origin = ImGui.GetCursorScreenPos();
            var outer = ImGui.GetContentRegionAvail();
            ImGui.SetScrollX(0f);
            ImGui.SetScrollY(0f);
            DrawPanel(origin, origin + outer, rounding);

            var gridWidth = MathF.Max(1f, outer.X - inset * 2f - vBar);
            var widths = ResolveWidths(columns, gridWidth, out var contentWidth);
            _columns = columns as MirageTableColumn[] ?? columns.ToArray();

            var scrollable = contentWidth > gridWidth + 0.5f;
            var hBar = scrollable ? vBar : 0f;
            var contentLeft = origin.X + inset;
            var contentTop = origin.Y + inset;
            var contentBottom = origin.Y + outer.Y - inset - hBar;
            var scrollX = 0f;
            if (scrollable)
            {
                ImGui.SetCursorScreenPos(new Vector2(contentLeft, origin.Y + outer.Y - inset - hBar));
                scrollX = DrawHorizontalBar(id, gridWidth, contentWidth);
            }

            Width = contentWidth;
            VisibleWidth = gridWidth;
            ScrollX = scrollX;

            ImGui.SetCursorScreenPos(new Vector2(contentLeft - scrollX, contentTop));
            Clip(
                new Vector2(contentLeft, contentTop),
                new Vector2(contentLeft + gridWidth, origin.Y + outer.Y),
                () => DrawColumns(id + "##head", contentWidth, widths, columns, header: true, null));
            var headerH = MathF.Max(1f, ImGui.GetCursorScreenPos().Y - contentTop);

            var footerH = drawAddRow == null
                ? 0f
                : ResolveControlHeight() + style.CellPadding.Y * 2f + 6f;
            var bodyTop = contentTop + headerH;
            DrawRule(contentLeft, contentLeft + gridWidth, bodyTop);
            var bodyH = MathF.Max(1f, contentBottom - footerH - bodyTop);

            var grab = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).Separator;
            grab.W = MathF.Max(grab.W, 0.35f);
            var grabHot = grab;
            grabHot.W = MathF.Min(1f, grab.W + 0.15f);
            var panel = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).FrameBg;
            using var scrollColors = new ImRaii.ColorDisposable()
                .Push(ImGuiCol.ScrollbarBg, panel)
                .Push(ImGuiCol.ScrollbarGrab, grab)
                .Push(ImGuiCol.ScrollbarGrabHovered, grabHot)
                .Push(ImGuiCol.ScrollbarGrabActive, grabHot);
            using var scrollRounding = ImRaii.PushStyle(ImGuiStyleVar.ScrollbarRounding, rounding);
            ImGui.SetCursorScreenPos(new Vector2(contentLeft, bodyTop));
            using (var body = ImRaii.Child(
                       id + "##rows",
                       new Vector2(gridWidth + vBar, bodyH),
                       false,
                       ImGuiWindowFlags.AlwaysVerticalScrollbar
                       | ImGuiWindowFlags.NoBackground
                       | ImGuiWindowFlags.NoSavedSettings))
            {
                if (body)
                {
                    if (rowCount > 0)
                    {
                        ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos() + new Vector2(-scrollX, 0f));
                        DrawColumns(id + "##body", contentWidth, widths, columns, header: false, () =>
                        {
                            for (var i = 0; i < rowCount; i++)
                            {
                                PopAlign();
                                _column = 0;
                                ImGui.TableNextRow();
                                if (drawRow(i))
                                    break;
                            }

                            PopAlign();
                        });
                    }
                }
            }

            if (drawAddRow != null)
            {
                var footerTop = bodyTop + bodyH;
                DrawRule(contentLeft, contentLeft + gridWidth, footerTop);
                ImGui.SetCursorScreenPos(new Vector2(contentLeft - scrollX, footerTop));
                Clip(
                    new Vector2(contentLeft, footerTop),
                    new Vector2(contentLeft + gridWidth, footerTop + footerH),
                    () => DrawColumns(id + "##add", contentWidth, widths, columns, header: false, () =>
                    {
                        PopAlign();
                        _column = 0;
                        ImGui.TableNextRow();
                        drawAddRow();
                        PopAlign();
                    }));
            }
        }

        private static void Clip(Vector2 min, Vector2 max, Action draw)
        {
            ImGui.PushClipRect(min, max, true);
            draw();
            ImGui.PopClipRect();
        }

        private static void DrawPanel(Vector2 min, Vector2 max, float rounding)
        {
            var settings = MirageTheme.Active ?? MirageTheme.ResolveAppliedColors();
            var drawList = ImGui.GetWindowDrawList();
            drawList.AddRectFilled(min, max, ImGui.GetColorU32(settings.FrameBg), rounding);
            drawList.AddRect(min, max, ImGui.GetColorU32(settings.Separator), rounding);
        }

        private static void DrawRule(float x0, float x1, float y)
        {
            var separator = (MirageTheme.Active ?? MirageTheme.ResolveAppliedColors()).Separator;
            ImGui.GetWindowDrawList().AddLine(
                new Vector2(x0, y),
                new Vector2(x1, y),
                ImGui.GetColorU32(separator));
        }

        private static float DrawHorizontalBar(string id, float viewWidth, float contentWidth)
        {
            using var bar = ImRaii.Child(
                id + "##h",
                new Vector2(viewWidth, ImGui.GetStyle().ScrollbarSize),
                false,
                ImGuiWindowFlags.HorizontalScrollbar
                | ImGuiWindowFlags.AlwaysHorizontalScrollbar
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoSavedSettings);
            if (!bar)
                return 0f;

            ImGui.Dummy(new Vector2(contentWidth, 1f));
            return ImGui.GetScrollX();
        }

        private static void DrawColumns(
            string id,
            float contentWidth,
            float[] widths,
            IReadOnlyList<MirageTableColumn> columns,
            bool header,
            Action? drawRows)
        {
            if (!ImGui.BeginTable(id, columns.Count, TableFlags, new Vector2(contentWidth, 0f)))
                return;

            try
            {
                for (var i = 0; i < columns.Count; i++)
                {
                    var label = header ? columns[i].Label : $"##c{i}";
                    ImGui.TableSetupColumn(label, ImGuiTableColumnFlags.WidthFixed, widths[i]);
                }

                if (header)
                    DrawHeader(columns);
                else
                    drawRows?.Invoke();
            }
            finally
            {
                ImGui.EndTable();
            }
        }

        private static void DrawHeader(IReadOnlyList<MirageTableColumn> columns)
        {
            ImGui.TableNextRow();
            using var headerText = ImRaii.PushColor(ImGuiCol.Text, GetColor(Color.Secondary));
            for (var i = 0; i < columns.Count; i++)
            {
                ImGui.TableSetColumnIndex(i);
                var label = columns[i].Label;
                if (string.IsNullOrEmpty(label))
                    continue;

                if (columns[i].Align == MirageTableAlign.Center)
                {
                    var text = ImGui.CalcTextSize(label).X;
                    var avail = ImGui.GetContentRegionAvail().X;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0f, (avail - text) * 0.5f));
                }

                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(label);
            }
        }

        private static void PopAlign()
        {
            if (_alignVars == 0)
                return;

            ImGui.PopStyleVar(_alignVars);
            _alignVars = 0;
        }

        private static float[] ResolveWidths(
            IReadOnlyList<MirageTableColumn> columns,
            float viewWidth,
            out float outerWidth)
        {
            var pad = ImGui.GetStyle().CellPadding.X * 2f;
            var chrome = pad * columns.Count;
            var available = MathF.Max(1f, viewWidth - chrome);
            var widths = new float[columns.Count];
            var fixedSum = 0f;
            var stretchCount = 0;
            var stretchMin = 0f;
            for (var i = 0; i < columns.Count; i++)
            {
                var width = MathF.Max(1f, columns[i].Width);
                widths[i] = width;
                if (columns[i].Stretch)
                {
                    stretchCount++;
                    stretchMin += width;
                }
                else
                {
                    fixedSum += width;
                }
            }

            if (stretchCount > 0)
            {
                var extra = MathF.Max(0f, available - fixedSum - stretchMin);
                var share = extra / stretchCount;
                for (var i = 0; i < columns.Count; i++)
                {
                    if (columns[i].Stretch)
                        widths[i] += share;
                }
            }

            outerWidth = chrome;
            foreach (var width in widths)
                outerWidth += width;
            return widths;
        }
    }
}
