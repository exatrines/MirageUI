using Dalamud.Interface.Textures.TextureWraps;
using MirageUI.Theme;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    public static bool Image(
        string path,
        float width,
        float height,
        bool isCircle = false,
        bool isCentering = false,
        float rounding = 0f)
    {
        if (!ImageRegistry.TryGetWrap(path, out var texture))
            return false;

        return DrawSizedImage(texture, width, height, isCircle, isCentering, rounding);
    }

    public static bool GameIcon(
        uint iconId,
        float width,
        float height,
        bool hiRes = true,
        bool isCircle = false,
        bool isCentering = false,
        bool framed = false)
    {
        if (!framed)
        {
            if (!ImageRegistry.TryGetGameIcon(iconId, hiRes, out var texture))
                return false;

            return DrawSizedImage(texture, width, height, isCircle, isCentering, rounding: 0f);
        }

        var size = new Vector2(width, height);
        if (isCentering)
        {
            var cursor = MirageLayout.Cursor.Position;
            var avail = MirageLayout.Style.ContentRegionAvail;
            MirageLayout.Cursor.Position = cursor + new Vector2((avail.X - width) * 0.5f, 0f);
        }

        IDalamudTextureWrap? framedTexture = null;
        var found = false;
        if (iconId != 0 && TryGetGameIcon(iconId, out var loaded, hiRes))
        {
            framedTexture = loaded;
            found = true;
        }

        var min = ImGui.GetCursorScreenPos();
        ImageFrame(framedTexture, min, min + size);
        ImGui.Dummy(size);
        return found;
    }

    public static bool FramedGameIcon(uint iconId, Vector2 min, Vector2 max, bool muted = false)
    {
        IDalamudTextureWrap? texture = null;
        if (iconId != 0 && TryGetGameIcon(iconId, out var loaded, hiRes: false))
            texture = loaded;

        ImageFrame(texture, min, max, Vector2.Zero, Vector2.One, muted, GameIconFit);
        return texture != null;
    }

    public static void ImageFrame(IDalamudTextureWrap? texture, Vector2 min, Vector2 max) =>
        ImageFrame(texture, min, max, Vector2.Zero, Vector2.One, muted: false);

    public static void ImageFrame(
        IDalamudTextureWrap? texture,
        Vector2 min,
        Vector2 max,
        Vector2 uv0,
        Vector2 uv1,
        bool muted,
        float fit = 1f)
    {
        var settings = MirageTheme.Active ?? ResolveAppliedColors();
        var accent = settings.GetColor(Color.Accent);
        var (fill, _, _, border, _, _, _, _) = GetDropdownColors(settings, accent);
        var rounding = InputStyle.FrameRounding;
        var draw = ImGui.GetWindowDrawList();
        draw.AddRectFilled(min, max, ToUInt(fill), rounding);

        if (texture != null)
        {
            var pad = new Vector2(1f, 1f);
            var innerMin = min + pad;
            var innerMax = max - pad;
            if (fit > 0f && fit < 1f)
            {
                var inner = innerMax - innerMin;
                var drawn = inner * fit;
                var origin = innerMin + (inner - drawn) * 0.5f;
                innerMin = origin;
                innerMax = origin + drawn;
            }

            if (innerMax.X - innerMin.X >= 1f && innerMax.Y - innerMin.Y >= 1f)
            {
                var tint = muted
                    ? ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.35f))
                    : uint.MaxValue;
                var radius = MathF.Max(0f, rounding - (innerMin.X - min.X));
                if (radius >= 0.5f)
                {
                    draw.AddImageRounded(
                        texture.Handle,
                        innerMin,
                        innerMax,
                        uv0,
                        uv1,
                        tint,
                        radius,
                        ImDrawFlags.RoundCornersAll);
                }
                else
                {
                    draw.AddImage(texture.Handle, innerMin, innerMax, uv0, uv1, tint);
                }
            }
        }

        draw.AddRect(min, max, ToUInt(border), rounding, ImDrawFlags.RoundCornersAll, 1f);
    }

    private const float GameIconFit = 0.94f;

    private static bool DrawSizedImage(
        IDalamudTextureWrap texture,
        float width,
        float height,
        bool isCircle,
        bool isCentering,
        float rounding)
    {
        var size = new Vector2(width, height);
        if (isCentering)
        {
            var cursor = MirageLayout.Cursor.Position;
            var avail = MirageLayout.Style.ContentRegionAvail;
            MirageLayout.Cursor.Position = cursor + new Vector2((avail.X - width) * 0.5f, 0f);
        }

        DrawImage(texture, size, isCircle, rounding);
        return true;
    }

    public static bool TryGetGameIcon(uint iconId, out IDalamudTextureWrap texture, bool hiRes = true) =>
        ImageRegistry.TryGetGameIcon(iconId, hiRes, out texture!);

    public static bool TryGetTexture(string path, out IDalamudTextureWrap texture) =>
        ImageRegistry.TryGetWrap(path, out texture);

    private static void DrawImage(IDalamudTextureWrap texture, Vector2 size, bool isCircle, float rounding)
    {
        var topLeft = ImGui.GetCursorScreenPos();
        var bottomRight = topLeft + size;
        var radius = isCircle ? MathF.Min(size.X, size.Y) * 0.5f : rounding;
        if (radius > 0f)
        {
            ImGui.GetWindowDrawList().AddImageRounded(
                texture.Handle,
                topLeft,
                bottomRight,
                Vector2.Zero,
                Vector2.One,
                uint.MaxValue,
                radius,
                ImDrawFlags.RoundCornersAll);
            ImGui.Dummy(size);
            return;
        }

        ImGui.Image(texture.Handle, size);
    }
}
