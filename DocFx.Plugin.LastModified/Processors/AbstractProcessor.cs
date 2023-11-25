namespace DocFx.Plugin.LastModified.Processors;

using Docfx.Common;
using Docfx.Plugins;
using HtmlAgilityPack;

/// <summary>
/// An abstract processor for adding last modified date to articles, contains common methods.
/// </summary>
public abstract class AbstractProcessor : IProcessor
{
    /// <summary>
    /// Determines whether this processor supports the given type.
    /// </summary>
    /// <param name="type">The type of manifest item.</param>
    /// <returns>True if this processor supports the given type, false otherwise.</returns>
    public abstract bool Supports(string type);

    /// <summary>
    /// Processes the given manifest item.
    /// </summary>
    /// <param name="manifest">The <see cref="Manifest"/> that contains the manifest item.</param>
    /// <param name="manifestItem">The <see cref="ManifestItem"/> to process.</param>
    /// <param name="outputFolder">The output folder.</param>
    public abstract void Process(Manifest manifest, ManifestItem manifestItem, string outputFolder);

    /// <summary>
    /// Attempts to process the given manifest item.
    /// </summary>
    /// <param name="manifest">The <see cref="Manifest"/> that contains the manifest item.</param>
    /// <param name="manifestItem">The <see cref="ManifestItem"/> to process.</param>
    /// <param name="outputFolder">The output folder.</param>
    /// <returns>True if the manifest item was processed, false otherwise.</returns>
    public bool TryProcess(Manifest manifest, ManifestItem manifestItem, string outputFolder)
    {
        if (!Supports(manifestItem.Type))
        {
            Logger.LogVerbose($"Skipping {manifestItem.Type}...");
            return false;
        }

        Logger.LogDiagnostic($"Processing {manifestItem.Type}...");
        Process(manifest, manifestItem, outputFolder);
        return true;
    }

    /// <summary>
    /// Builds the last modified info for the given file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The <see cref="LastModifiedInfo"/> for the given file path.</returns>
    protected abstract LastModifiedInfo GetLastModifiedInfo(string filePath);

    /// <summary>
    /// Modifies the given document with the given last modified info.
    /// </summary>
    /// <param name="filePath">The file path to the document.</param>
    /// <param name="lastModifiedInfo">The <see cref="LastModifiedInfo"/> to use.</param>
    protected virtual void ModifyDocument(string filePath, LastModifiedInfo lastModifiedInfo)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(filePath);

        var articleNode = htmlDocument.DocumentNode.SelectSingleNode("//article");
        if (articleNode == null)
        {
            Logger.LogWarning($"No article node found in {filePath}.");
            return;
        }

        var separatorNode = htmlDocument.CreateElement("hr");
        articleNode.AppendChild(separatorNode);

        var lastModifiedNode = htmlDocument.CreateElement("div");
        lastModifiedNode.SetAttributeValue("class", "last-modified");
        articleNode.AppendChild(lastModifiedNode);

        var paragraphNode = htmlDocument.CreateElement("p");
        paragraphNode.InnerHtml = $"This page was last modified at {lastModifiedInfo.LastModified} (UTC).";
        lastModifiedNode.AppendChild(paragraphNode);

        if (string.IsNullOrEmpty(lastModifiedInfo.CommitHeader))
        {
            htmlDocument.Save(filePath);
            return;
        }

        var collapsibleNode = htmlDocument.CreateElement("details");
        lastModifiedNode.AppendChild(collapsibleNode);

        var reasonHeaderNode = htmlDocument.CreateElement("summary");
        reasonHeaderNode.SetAttributeValue("style", "display: list-item;");
        reasonHeaderNode.InnerHtml = "Commit Message";
        collapsibleNode.AppendChild(reasonHeaderNode);

        var reasonContainerNode = htmlDocument.CreateElement("div");
        collapsibleNode.AppendChild(reasonContainerNode);

        var preCodeBlockNode = htmlDocument.CreateElement("pre");
        var codeBlockNode = htmlDocument.CreateElement("code");
        codeBlockNode.InnerHtml = lastModifiedInfo.CommitHeader;
        preCodeBlockNode.AppendChild(codeBlockNode);
        reasonContainerNode.AppendChild(preCodeBlockNode);

        if (!string.IsNullOrEmpty(lastModifiedInfo.CommitBody))
        {
            preCodeBlockNode = htmlDocument.CreateElement("pre");
            codeBlockNode = htmlDocument.CreateElement("code");
            codeBlockNode.InnerHtml = lastModifiedInfo.CommitBody;
            preCodeBlockNode.AppendChild(codeBlockNode);
            reasonContainerNode.AppendChild(preCodeBlockNode);
        }

        htmlDocument.Save(filePath);
    }
}
