using System.IO;

namespace MirageUI;

internal static class MiragePluginPage
{
    private const float IconSize = 96f;
    private const int SupportHighlightFrames = 90;

    private sealed class HostState
    {
        public bool Visible;
        public bool FocusSupport;
        public int FocusSupportFrames;
    }

    private static readonly Dictionary<uint, HostState> Hosts = [];
    private static uint _drawingHostId;

    internal static void BeginHost() =>
        _drawingHostId = ImGuiP.GetCurrentWindow().ID;

    internal static void EndHost() =>
        _drawingHostId = 0;

    internal static bool IsVisible =>
        _drawingHostId != 0
        && Hosts.TryGetValue(_drawingHostId, out var state)
        && state.Visible;

    internal static void Toggle()
    {
        if (_drawingHostId == 0)
            return;

        var state = Get(_drawingHostId);
        state.Visible = !state.Visible;
        state.FocusSupport = false;
        state.FocusSupportFrames = 0;
    }

    internal static void OpenCurrent(bool focusSupport)
    {
        var hostId = _drawingHostId != 0 ? _drawingHostId : ImGuiP.GetCurrentWindow().ID;
        if (hostId == 0)
            return;

        var state = Get(hostId);
        state.Visible = true;
        if (!focusSupport)
            return;

        state.FocusSupport = true;
        state.FocusSupportFrames = SupportHighlightFrames;
    }

    internal static void Hide()
    {
        if (_drawingHostId == 0 || !Hosts.TryGetValue(_drawingHostId, out var state))
            return;

        state.Visible = false;
        state.FocusSupport = false;
        state.FocusSupportFrames = 0;
    }

    internal static void Dispose()
    {
        Hosts.Clear();
        _drawingHostId = 0;
    }

    internal static void Draw()
    {
        var info = MirageUi.PluginInfo;
        var highlightSupport = false;
        if (_drawingHostId != 0 && Hosts.TryGetValue(_drawingHostId, out var host))
        {
            highlightSupport = host.FocusSupport && host.FocusSupportFrames > 0;
            if (host.FocusSupportFrames > 0)
                host.FocusSupportFrames--;
            else
                host.FocusSupport = false;
        }

        var scale = MirageLayout.Style.Scale;
        var icon = IconSize * scale;
        var startX = MirageLayout.Cursor.Position.X;
        var columnWidth = MirageLayout.Style.ContentRegionAvail.X;
        var blockHeight = EstimateBlockHeight(info, icon, scale, columnWidth);
        var availY = MirageLayout.Style.ContentRegionAvail.Y;
        MirageLayout.Cursor.Y += Math.Max(0f, (availY - blockHeight) * 0.5f);

        DrawCenteredIcon(info.IconPath, icon, startX, columnWidth);
        MirageLayout.Cursor.Y += 12f * scale;

        DrawCenteredLine(info.Name.Trim(), MirageUi.Color.Title, MirageUi.FontSize.Large, startX, columnWidth);
        DrawCenteredLine(FormatVersion(info.Version), MirageUi.Color.Secondary, MirageUi.FontSize.Default, startX, columnWidth);
        DrawCenteredLine(FormatAuthor(info), MirageUi.Color.Secondary, MirageUi.FontSize.Default, startX, columnWidth);

        var message = info.Message.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            MirageLayout.Cursor.Y += 8f * scale;
            DrawCenteredWrapped(message, startX, columnWidth);
        }

        if (HasAnyLink(info))
        {
            MirageLayout.Cursor.Y += 20f * scale;
            DrawLinkRow(info, startX, columnWidth, highlightSupport);
        }
    }

    private static HostState Get(uint hostId)
    {
        if (Hosts.TryGetValue(hostId, out var state))
            return state;

        state = new HostState();
        Hosts[hostId] = state;
        return state;
    }

    private static string FormatAuthor(MiragePluginInfo info)
    {
        var author = info.Publisher.Trim();
        return string.IsNullOrEmpty(author) ? string.Empty : $"by {author}";
    }

    private static string FormatVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return string.Empty;

        return version.StartsWith('v') || version.StartsWith('V')
            ? version
            : "v" + version;
    }

    private static float EstimateBlockHeight(MiragePluginInfo info, float icon, float scale, float columnWidth)
    {
        var height = icon + 12f * scale;
        if (HasAnyLink(info))
            height += 20f * scale + MirageUi.GetLinkButtonHeight();
        var lineGap = MirageLayout.Style.ItemSpacing.Y;
        var name = info.Name.Trim();
        using (MirageUi.PushFont(MirageUi.FontSize.Large))
        {
            if (!string.IsNullOrEmpty(name))
                height += ImGui.CalcTextSize(name).Y + lineGap;
        }

        using (MirageUi.PushFont(MirageUi.FontSize.Default))
        {
            var version = FormatVersion(info.Version);
            if (!string.IsNullOrEmpty(version))
                height += ImGui.CalcTextSize(version).Y + lineGap;

            var author = FormatAuthor(info);
            if (!string.IsNullOrEmpty(author))
                height += ImGui.CalcTextSize(author).Y + lineGap;

            var message = info.Message.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                height += 8f * scale;
                var wrapWidth = GetMessageWrapWidth(columnWidth);
                var fullWidth = ImGui.CalcTextSize(message).X;
                var lineH = ImGui.GetTextLineHeight();
                var lines = wrapWidth <= 0f ? 1 : Math.Max(1, (int)Math.Ceiling(fullWidth / wrapWidth));
                height += lines * lineH + lineGap;
            }
        }

        return height;
    }

    private static void DrawCenteredIcon(string? path, float icon, float startX, float columnWidth)
    {
        var y = MirageLayout.Cursor.Position.Y;
        MirageLayout.Cursor.Position = new Vector2(CenteredX(startX, columnWidth, icon), y);
        var drawn = !string.IsNullOrWhiteSpace(path)
            && File.Exists(path)
            && MirageUi.Image(path, icon, icon);
        if (!drawn)
            ImGui.Dummy(new Vector2(icon, icon));

        MirageLayout.Cursor.Position = new Vector2(startX, y + icon);
    }

    private static void DrawCenteredLine(
        string text,
        MirageUi.Color color,
        MirageUi.FontSize fontSize,
        float startX,
        float columnWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        using (MirageUi.PushFont(fontSize))
        {
            var width = ImGui.CalcTextSize(text).X;
            MirageLayout.Cursor.X = CenteredX(startX, columnWidth, width);
            MirageUi.Text(text, color, wrap: false, fontSize: fontSize);
            MirageLayout.Cursor.X = startX;
        }
    }

    private static void DrawCenteredWrapped(string text, float startX, float columnWidth)
    {
        var wrapWidth = GetMessageWrapWidth(columnWidth);
        using (MirageUi.PushFont(MirageUi.FontSize.Default))
        {
            var textWidth = ImGui.CalcTextSize(text).X;
            var drawWidth = wrapWidth <= 0f ? textWidth : Math.Min(textWidth, wrapWidth);
            MirageLayout.Cursor.X = CenteredX(startX, columnWidth, drawWidth);
            if (textWidth > wrapWidth && wrapWidth > 0f)
            {
                ImGui.PushTextWrapPos(ImGui.GetCursorScreenPos().X + wrapWidth);
                MirageUi.Text(text, MirageUi.Color.Secondary, wrap: true);
                ImGui.PopTextWrapPos();
            }
            else
            {
                MirageUi.Text(text, MirageUi.Color.Secondary, wrap: false);
            }

            MirageLayout.Cursor.X = startX;
        }
    }

    private static float GetMessageWrapWidth(float columnWidth)
    {
        var max = 360f * MirageLayout.Style.Scale;
        return Math.Min(columnWidth, max);
    }

    private static float CenteredX(float startX, float columnWidth, float width) =>
        startX + Math.Max(0f, (columnWidth - width) * 0.5f);

    private static bool HasAnyLink(MiragePluginInfo info) =>
        !string.IsNullOrWhiteSpace(info.RepoUrl)
        || !string.IsNullOrWhiteSpace(info.DiscordUrl)
        || !string.IsNullOrWhiteSpace(info.SupportUrl);

    private static void DrawLinkRow(MiragePluginInfo info, float startX, float columnWidth, bool highlightSupport)
    {
        var links = new List<(string Label, string Url, bool Highlight)>(3);
        if (!string.IsNullOrWhiteSpace(info.RepoUrl))
            links.Add(("GitHub", info.RepoUrl.Trim(), false));
        if (!string.IsNullOrWhiteSpace(info.DiscordUrl))
            links.Add(("Discord", info.DiscordUrl.Trim(), false));
        if (!string.IsNullOrWhiteSpace(info.SupportUrl))
            links.Add(("Support", info.SupportUrl.Trim(), highlightSupport));
        if (links.Count == 0)
            return;

        var gap = Math.Max(MirageLayout.Style.ItemInnerSpacing.X, 8f);
        var total = 0f;
        for (var i = 0; i < links.Count; i++)
        {
            total += MeasureLinkWidth(links[i].Label);
            if (i < links.Count - 1)
                total += gap;
        }

        MirageLayout.Cursor.X = CenteredX(startX, columnWidth, total);

        for (var i = 0; i < links.Count; i++)
        {
            if (i > 0)
                ImGui.SameLine(0f, gap);
            DrawLinkButton(links[i].Label, links[i].Url, links[i].Highlight);
        }
    }

    private static float MeasureLinkWidth(string label)
    {
        using (MirageUi.PushFont(MirageUi.FontSize.Default))
            return ImGui.CalcTextSize(label).X + MirageUi.GetLinkButtonHorizontalPadding() * 2f;
    }

    private static void DrawLinkButton(string label, string? url, bool highlight)
    {
        var enabled = !string.IsNullOrWhiteSpace(url);
        if (highlight)
        {
            var accent = MirageUi.GetColor(MirageUi.Color.Accent);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(accent.X, accent.Y, accent.Z, 0.40f));
            ImGui.PushStyleColor(ImGuiCol.Border, accent);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2f);
        }

        using (MirageUi.DisabledIf(!enabled))
            MirageUi.Link(label, url ?? string.Empty);

        if (highlight)
        {
            ImGui.SetItemDefaultFocus();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);
        }
    }
}
