namespace sharpfetch.Configuration;

/// <summary>Controls which icon set is used when <see cref="DisplayConfiguration.ShowIcons"/> is <c>true</c>.</summary>
public enum IconStyle
{
    /// <summary>Standard Unicode emoji icons — work in every modern terminal.</summary>
    Emoji,
    /// <summary>
    /// Nerd Font glyph icons — requires a Nerd Font to be installed and selected
    /// in the terminal (e.g. FiraCode Nerd Font, JetBrainsMono Nerd Font).
    /// </summary>
    NerdFont
}
