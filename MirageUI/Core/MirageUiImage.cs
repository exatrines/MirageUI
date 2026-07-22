using Dalamud.Interface.Textures.TextureWraps;
using MirageUI.Ui;

namespace MirageUI;

public static partial class MirageUi
{
    /// <summary>画像を表示する。テクスチャ未ロード時は false。</summary>
    public static bool Image(
        string path,
        float width,
        float height,
        bool isCircle = false,
        bool isCentering = false)
    {
        if (!ImageRegistry.TryGetWrap(path, out var texture))
            return false;

        return DrawSizedImage(texture, width, height, isCircle, isCentering);
    }

    /// <summary>ゲームアイコンを表示する。テクスチャ未ロード時は false。</summary>
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

        return DrawSizedImage(texture, width, height, isCircle, isCentering);
    }

    private static bool DrawSizedImage(
        IDalamudTextureWrap texture,
        float width,
        float height,
        bool isCircle,
        bool isCentering)
    {
        var size = new Vector2(width, height);
        if (isCentering)
        {
            var cursor = MirageLayout.Cursor.Position;
            var avail = MirageLayout.Style.ContentRegionAvail;
            MirageLayout.Cursor.Position = cursor + new Vector2((avail.X - width) * 0.5f, 0f);
        }

        DrawImage(texture, size, isCircle);
        return true;
    }

    /// <summary>ゲームアイコンのテクスチャを取得する。未ロード時は false。</summary>
    public static bool TryGetGameIcon(uint iconId, out IDalamudTextureWrap texture, bool hiRes = true) =>
        ImageRegistry.TryGetGameIcon(iconId, hiRes, out texture!);

    private static void DrawImage(IDalamudTextureWrap texture, Vector2 size, bool isCircle)
    {
        var topLeft = ImGui.GetCursorScreenPos();
        var bottomRight = topLeft + size;

        if (isCircle)
        {
            var rounding = MathF.Min(size.X, size.Y) * 0.5f;
            ImGui.GetWindowDrawList().AddImageRounded(
                texture.Handle,
                topLeft,
                bottomRight,
                Vector2.Zero,
                Vector2.One,
                uint.MaxValue,
                rounding,
                ImDrawFlags.RoundCornersAll);
            ImGui.Dummy(size);
            return;
        }

        ImGui.Image(texture.Handle, size);
    }
}
