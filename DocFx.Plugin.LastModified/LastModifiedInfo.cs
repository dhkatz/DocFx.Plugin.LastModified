namespace DocFx.Plugin.LastModified;

using System;

public record LastModifiedInfo
{
    /// <summary>
    /// Gets the last modified date of the file.
    /// </summary>
    public DateTimeOffset LastModified { get; init; }

    /// <summary>
    /// Gets the commit header, if any.
    /// </summary>
    public string CommitHeader { get; init; } = string.Empty;

    /// <summary>
    /// Gets the commit body, if any.
    /// </summary>
    public string CommitBody { get; init; } = string.Empty;
}
