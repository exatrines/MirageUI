using Dalamud.Interface.Textures.TextureWraps;
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
        bool isCentering = false)
    {
        if (!ImageRegistry.TryGetGameIcon(iconId, hiRes, out var texture))
            return false;

        return DrawSizedImage(texture, width, height, isCircle, isCentering, rounding: 0f);
    }

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
