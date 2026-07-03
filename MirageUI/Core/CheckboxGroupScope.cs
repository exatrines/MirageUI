namespace MirageUI;

/// <summary>
/// <see cref="MirageUi.CheckboxGroup"/> 呼び出し後のインデントを解除する。
/// </summary>
public readonly struct CheckboxGroupScope(bool changed) : IDisposable
{
    public bool Changed => changed;

    public void Dispose() => ImGui.Unindent();
}
