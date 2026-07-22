namespace MirageUI.Layout;

public sealed class MirageTwoColumnRunBinding
{
    private bool _isRunning;

    public ref bool IsRunning => ref _isRunning;

    public Action? OnRun { get; set; }

    public string RunTooltip { get; set; } = "Run";

    public string StopTooltip { get; set; } = "Stop";

    public MirageTooltipPosition TooltipPosition { get; set; } = MirageTooltipPosition.Cursor;
}
