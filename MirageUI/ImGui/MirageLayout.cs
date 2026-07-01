using Dalamud.Interface.Utility;

namespace MirageUI.Ui;

public static class MirageLayout
{
    public static class Style
    {
        public static float Scale => ImGuiHelpers.GlobalScale;
        public static float FrameHeight => ImGui.GetFrameHeight();
        public static Vector2 ContentRegionAvail => ImGui.GetContentRegionAvail();
        public static Vector2 ContentRegionMax => ImGui.GetContentRegionMax();
        public static float WindowRounding => ImGui.GetStyle().WindowRounding;
        public static Vector2 FramePadding => ImGui.GetStyle().FramePadding;
        public static Vector2 ItemSpacing => ImGui.GetStyle().ItemSpacing;
        public static Vector2 ItemInnerSpacing => ImGui.GetStyle().ItemInnerSpacing;
        public static Vector2 SelectableTextAlign => ImGui.GetStyle().SelectableTextAlign;
    }

    public static class Cursor
    {
        public static Vector2 Position
        {
            get => ImGui.GetCursorPos();
            set => ImGui.SetCursorPos(value);
        }

        public static float X
        {
            get => ImGui.GetCursorPosX();
            set => ImGui.SetCursorPosX(value);
        }

        public static float Y
        {
            get => ImGui.GetCursorPosY();
            set => ImGui.SetCursorPosY(value);
        }

        public static Vector2 ScreenPosition
        {
            get => ImGui.GetCursorScreenPos();
            set => ImGui.SetCursorScreenPos(value);
        }
    }
}

public static class MirageLayoutExtensions
{
    public static Vector2 XOnly(this Vector2 value) => new(value.X, 0f);
}
