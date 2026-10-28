namespace MirageUI;

public readonly struct DisabledScope(bool active) : IDisposable
{
    public void Dispose()
    {
        if (active)
            ImGui.EndDisabled();
    }
}
