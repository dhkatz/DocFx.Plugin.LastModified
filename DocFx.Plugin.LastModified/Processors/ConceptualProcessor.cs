namespace DocFx.Plugin.LastModified.Processors;

using System;
using System.IO;
using System.Text;
using Docfx.Common;
using Docfx.Plugins;
using Helpers;
using LibGit2Sharp;

/// <summary>
/// Processor for adding last modified date to conceptual articles.
/// </summary>
public class ConceptualProcessor : AbstractProcessor
{
    private Repository? _repo;

    /// <inheritdoc />
    public override bool Supports(string type)
    {
        return type == "Conceptual";
    }

    /// <inheritdoc />
    public override void Process(Manifest manifest, ManifestItem manifestItem, string outputFolder)
    {
        var repository = Repository.Discover(manifest.SourceBasePath);
        if (repository != null)
        {
            _repo = new Repository(repository);
        }

        var sourcePath = Path.Combine(manifest.SourceBasePath, manifestItem.SourceRelativePath);
        var outputPath = Path.Combine(outputFolder, manifestItem.Output[".html"].RelativePath);

        var lastModifiedInfo = GetLastModifiedInfo(sourcePath);

        ModifyDocument(outputPath, lastModifiedInfo);
    }

    /// <inheritdoc />
    protected override LastModifiedInfo GetLastModifiedInfo(string filePath)
    {
        var lastModified = DateTimeOffset.MinValue;
        var commitHeader = string.Empty;
        var commitBody = string.Empty;

        if (_repo?.GetCommitInfo(filePath) is { } commitInfo)
        {
            lastModified = commitInfo.Author.When;
            Logger.LogDiagnostic($"Last modified date: {lastModified} (UTC)");

            var commitHeaderBuilder = new StringBuilder();
            commitHeaderBuilder.AppendLine($"Author:    {commitInfo.Author.Name}");
            commitHeaderBuilder.AppendLine($"Commit:    {commitInfo.Sha}");

            commitHeader = commitHeaderBuilder.ToString();
            commitBody = commitInfo.Message.Truncate(300);
        }

        if (lastModified == DateTimeOffset.MinValue)
        {
            lastModified = File.GetLastWriteTimeUtc(filePath);
            Logger.LogVerbose($"Last modified date: {lastModified} (UTC)");
        }

        return new LastModifiedInfo
        {
            LastModified = lastModified,
            CommitHeader = commitHeader,
            CommitBody = commitBody,
        };
    }
}
