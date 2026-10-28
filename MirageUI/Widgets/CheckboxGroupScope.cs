namespace MirageUI;

public readonly struct CheckboxGroupScope(bool changed) : IDisposable
{
    public bool Changed => changed;

    public void Dispose() => ImGui.Unindent();
}
