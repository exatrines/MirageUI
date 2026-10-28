using Dalamud.Interface;

namespace MirageUI;

public sealed class MirageMessageButton
{
    public required string Label { get; init; }

    public Action? OnClick { get; init; }

    public bool Primary { get; init; }

    public bool CloseOnClick { get; init; } = true;
}

public sealed class MirageMessageState
{
    public bool Visible { get; set; }

    public FontAwesomeIcon? Icon { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public IList<MirageMessageButton> Buttons { get; set; } = [];

    public void Show(
        string title,
        string message,
        FontAwesomeIcon? icon = null,
        params MirageMessageButton[] buttons)
    {
        Title = title;
        Message = message;
        Icon = icon;
        Buttons = buttons;
        Visible = true;
    }

    public void Hide() => Visible = false;
}
