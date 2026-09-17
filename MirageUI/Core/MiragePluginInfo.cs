using System.IO;
using Dalamud.Interface;
using Dalamud.Plugin;

namespace MirageUI;

public sealed class MiragePluginInfo
{
    public string Name { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? IconPath { get; set; }

    public string? RepoUrl { get; set; }

    public string? SupportUrl { get; set; }

    public string? DiscordUrl { get; set; }

    public FontAwesomeIcon TitleBarIcon { get; set; } = FontAwesomeIcon.Ghost;

    public static MiragePluginInfo FromPluginInterface(IDalamudPluginInterface pluginInterface)
    {
        ArgumentNullException.ThrowIfNull(pluginInterface);

        var manifest = pluginInterface.Manifest;
        var info = new MiragePluginInfo
        {
            Name = string.IsNullOrWhiteSpace(manifest.Name)
                ? pluginInterface.InternalName
                : manifest.Name,
            Publisher = manifest.Author ?? string.Empty,
            Version = manifest.AssemblyVersion.ToString(),
            RepoUrl = string.IsNullOrWhiteSpace(manifest.RepoUrl) ? null : manifest.RepoUrl.Trim(),
        };

        var dir = pluginInterface.AssemblyLocation.DirectoryName ?? AppContext.BaseDirectory;
        var iconPath = Path.Combine(dir, "Data", "plugin-icon.png");
        if (File.Exists(iconPath))
            info.IconPath = iconPath;

        return info;
    }
}
