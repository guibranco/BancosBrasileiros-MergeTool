namespace BancosBrasileiros.MergeTool.Dto;

using Helpers;

/// <summary>
/// Class ChangeModel.
/// </summary>
public class ChangeModel
{
    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    /// <value>The source.</value>
    public Source Source { get; set; }

    /// <summary>
    /// Gets or sets the old value.
    /// </summary>
    /// <value>The old value.</value>
    public string OldValue { get; set; }

    /// <summary>
    /// Creates new value.
    /// </summary>
    /// <value>The new value.</value>
    public string NewValue { get; set; }
}
