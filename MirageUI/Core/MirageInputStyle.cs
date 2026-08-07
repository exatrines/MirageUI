namespace MirageUI;

/// <summary>
/// Shared metrics for Mirage text fields, numeric inputs, and dropdowns.
/// Configure via <see cref="MirageUi.ConfigureInputStyle"/> / <see cref="MirageUi.SetInputStyle"/>.
/// </summary>
public sealed class MirageInputStyle
{
    /// <summary>
    /// Target outer control height. When set, vertical padding is derived so text stays
    /// vertically centered: <c>PaddingY = (Height - textLineHeight) / 2</c>.
    /// Default 26 keeps Mirage inputs / dropdowns aligned.
    /// </summary>
    public float? Height { get; set; } = 26f;

    /// <summary>Horizontal frame padding. Null keeps the current ImGui style value.</summary>
    public float? PaddingX { get; set; } = 8f;

    /// <summary>
    /// Vertical frame padding when <see cref="Height"/> is null.
    /// Null keeps the current ImGui style value.
    /// </summary>
    public float? PaddingY { get; set; }

    /// <summary>Frame corner rounding for inputs and dropdowns.</summary>
    public float FrameRounding { get; set; } = 4f;
}
