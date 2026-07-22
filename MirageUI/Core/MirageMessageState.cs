using Dalamud.Interface;

namespace MirageUI;

/// <summary>Message ダイアログのボタン（最大 3 つ）。</summary>
public sealed class MirageMessageButton
{
    public required string Label { get; init; }

    public Action? OnClick { get; init; }

    /// <summary>true でアクセント強調ボタン。</summary>
    public bool Primary { get; init; }

    /// <summary>クリック後にダイアログを閉じる（既定 true）。</summary>
    public bool CloseOnClick { get; init; } = true;
}

/// <summary>Message ダイアログの表示状態と内容。</summary>
public sealed class MirageMessageState
{
    public bool Visible { get; set; }

    public FontAwesomeIcon? Icon { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>最大 3 つまで表示される。</summary>
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
