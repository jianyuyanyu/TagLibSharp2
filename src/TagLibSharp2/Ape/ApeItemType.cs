// APE Tag Item Type enumeration
// Based on APEv2 specification: bits 2-1 of item flags

#pragma warning disable CA1700 // Do not name enum values 'Reserved'

namespace TagLibSharp2.Ape;

/// <summary>
/// Specifies the type of value stored in an APE tag item.
/// </summary>
public enum ApeItemType
{
    /// <summary>
    /// UTF-8 text string (flags bits 2-1 = 0).
    /// </summary>
    Text = 0,

    /// <summary>
    /// Binary data (flags bits 2-1 = 1).
    /// For cover art, format is: filename + null byte + binary data.
    /// </summary>
    Binary = 1,

    /// <summary>
    /// External locator/URI (flags bits 2-1 = 2).
    /// UTF-8 encoded URL or file path.
    /// </summary>
    ExternalLocator = 2,

    /// <summary>
    /// Reserved for future use (flags bits 2-1 = 3).
    /// </summary>
    Reserved = 3
}
