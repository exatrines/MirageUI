namespace MirageUI;

public static partial class MirageUi
{
    public static DisabledScope DisabledIf(bool disabled)
    {
        if (disabled)
            ImGui.BeginDisabled();

        return new DisabledScope(disabled);
    }
}
