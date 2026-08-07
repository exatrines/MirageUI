using MirageUI.Fonts;
using MirageUI.Theme;
using MirageUI.Ui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MirageUI;

public static partial class MirageUi
{
    private static MirageInputStyle _inputStyle = new();

    /// <summary>Current shared input / dropdown metrics.</summary>
    public static MirageInputStyle InputStyle => _inputStyle;

    internal static Func<MirageColorSettings> ResolveAppliedColors { get; private set; } =
        MirageColorSettings.CreateDefault;

    public static void ConfigureTheme(Func<MirageColorSettings> resolveAppliedColors) =>
        ResolveAppliedColors = resolveAppliedColors;

    /// <summary>Replace the shared input style settings.</summary>
    public static void SetInputStyle(MirageInputStyle style) =>
        _inputStyle = style ?? new MirageInputStyle();

    /// <summary>Mutate the shared input style settings.</summary>
    public static void ConfigureInputStyle(Action<MirageInputStyle> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_inputStyle);
    }

    /// <summary>
    /// Frame padding applied to Mirage inputs and dropdowns (text vertically centered via pad Y).
    /// </summary>
    public static Vector2 ResolveInputFramePadding()
    {
        var current = MirageLayout.Style.FramePadding;
        var padX = _inputStyle.PaddingX ?? current.X;

        if (_inputStyle.Height is { } height)
        {
            var textH = ImGui.GetTextLineHeight();
            var padY = MathF.Max(0f, (height - textH) * 0.5f);
            return new Vector2(padX, padY);
        }

        var padYDefault = _inputStyle.PaddingY ?? current.Y;
        return new Vector2(padX, padYDefault);
    }

    /// <summary>
    /// Outer control height shared by inputs, dropdowns, and buttons
    /// (<see cref="MirageInputStyle.Height"/>, or text line + resolved frame padding).
    /// </summary>
    public static float ResolveControlHeight()
    {
        if (_inputStyle.Height is { } height)
            return height;

        var pad = ResolveInputFramePadding();
        return ImGui.GetTextLineHeight() + pad.Y * 2f;
    }

    public static void Init(
        IDalamudPluginInterface pluginInterface,
        ITextureProvider? textureProvider = null,
        IPluginLog? log = null)
    {
        UiContext.Set(pluginInterface, log);
        FontRegistry.Init();
        if (textureProvider != null)
            ImageRegistry.Init(textureProvider);
    }

    public static void Dispose()
    {
        ImageRegistry.Dispose();
        FontRegistry.Dispose();
    }
}
