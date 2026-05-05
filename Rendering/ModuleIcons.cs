using sharpfetch.Configuration;

namespace sharpfetch.Rendering;

/// <summary>
/// Centralised icon look-up for both supported icon sets.
/// Add a new entry here whenever a new module is registered —
/// the renderer picks the right set automatically based on
/// <see cref="DisplayConfiguration.IconStyle"/>.
/// </summary>
public static class ModuleIcons
{
    /// <summary>
    /// Standard Unicode emoji icons.
    /// These render correctly in every modern terminal without any font installation.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Emoji =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["user"]          = "👤",
            ["os"]            = "🖥️",
            ["kernel"]        = "✅",
            ["cpu"]           = "🔲",
            ["memory"]        = "💾",
            ["gpu"]           = "🎮",
            ["bios"]          = "🔧",
            ["disk"]          = "💿",
            ["shell"]         = "🐚",
            ["terminal"]      = "📟",
            ["windowmanager"] = "🪟",
            ["uptime"]        = "🔥",
            ["datetime"]      = "🕐",
        };

    /// <summary>
    /// Nerd Font glyph icons (Nerd Fonts v3).
    /// Requires a Nerd Font (e.g. "FiraCode Nerd Font", "JetBrainsMono Nerd Font")
    /// to be installed and selected as the active terminal font.
    /// See https://www.nerdfonts.com for downloads.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> NerdFont =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["user"]          = "\uf007",   // nf-fa-user
            ["os"]            = "\uf109",   // nf-fa-laptop
            ["kernel"]        = "\uf17c",   // nf-fa-linux
            ["cpu"]           = "\uf2db",   // nf-fa-microchip
            ["memory"]        = "\uf538",   // nf-fa-memory
            ["gpu"]           = "\uf1b2",   // nf-fa-cube
            ["bios"]          = "\uf013",   // nf-fa-cog
            ["disk"]          = "\uf0a0",   // nf-fa-hdd_o
            ["shell"]         = "\ue795",   // nf-dev-terminal
            ["terminal"]      = "\uf120",   // nf-fa-terminal
            ["windowmanager"] = "\ufd2d",   // nf-md-window_maximize
            ["uptime"]        = "\uf252",   // nf-fa-hourglass_half
            ["datetime"]      = "\uf017",   // nf-fa-clock_o
        };

    /// <summary>Fallback glyph used when a module ID has no entry in the active icon set.</summary>
    private const string FallbackEmoji    = "•";
    private const string FallbackNerdFont = "\uf128"; // nf-fa-question

    /// <summary>
    /// Returns the icon for <paramref name="moduleId"/> from the icon set
    /// matching <paramref name="style"/>, falling back to a generic glyph when
    /// no entry exists for that module ID.
    /// </summary>
    public static string Get(string moduleId, IconStyle style)
    {
        var map = style == IconStyle.NerdFont ? NerdFont : Emoji;
        return map.TryGetValue(moduleId, out var icon)
            ? icon
            : style == IconStyle.NerdFont ? FallbackNerdFont : FallbackEmoji;
    }
}
