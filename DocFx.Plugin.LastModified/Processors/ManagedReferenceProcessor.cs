namespace DocFx.Plugin.LastModified.Processors;

using System;
using System.IO;
using System.Linq;
using Docfx.Common;
using Docfx.Plugins;
using Files;
using Helpers;
using LibGit2Sharp;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Processor for adding last modified date to managed reference articles.
/// </summary>
public class ManagedReferenceProcessor : AbstractProcessor
{
    private readonly Deserializer _deserializer = Deserializer.FromValueDeserializer(
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .BuildValueDeserializer());

    private Repository? _repo;
    private Manifest? _manifest;

    /// <inheritdoc />
    public override bool Supports(string type)
    {
        return type == "ManagedReference";
    }

    /// <inheritdoc />
    public override void Process(Manifest manifest, ManifestItem manifestItem, string outputFolder)
    {
        var repository = Repository.Discover(manifest.SourceBasePath);
        if (repository != null)
        {
            _repo = new Repository(repository);
        }

        _manifest = manifest;

        var sourcePath = Path.Combine(manifest.SourceBasePath, manifestItem.SourceRelativePath);
        var outputPath = Path.Combine(outputFolder, manifestItem.Output[".html"].RelativePath);

        var lastModifiedInfo = GetLastModifiedInfo(sourcePath);

        ModifyDocument(outputPath, lastModifiedInfo);
    }

    /// <inheritdoc />
    protected override LastModifiedInfo GetLastModifiedInfo(string filePath)
    {
        var yml = File.ReadAllText(filePath);
        var managedReferenceDocument = _deserializer.Deserialize<ManagedReferenceDocument?>(yml);
        var itemSource = managedReferenceDocument?.Items?.FirstOrDefault(i => !string.IsNullOrEmpty(i.Source?.Path));
        var itemSourcePath = itemSource?.Source?.Path ?? string.Empty;

        var lastModified = DateTimeOffset.MinValue;
        var commitHeader = string.Empty;
        var commitBody = string.Empty;

        var sourcePath = Path.Combine(_manifest?.SourceBasePath ?? string.Empty, itemSourcePath);

        if (!string.IsNullOrEmpty(itemSourcePath) && _repo?.GetCommitInfo(sourcePath) is { } commitInfo)
        {
            lastModified = commitInfo.Author.When;
            Logger.LogDiagnostic($"Last modified date: {lastModified} (UTC)");

            var commitHeaderBuilder = new System.Text.StringBuilder();
            commitHeaderBuilder.AppendLine($"Author:    {commitInfo.Author.Name}");
            commitHeaderBuilder.AppendLine($"Commit:    {commitInfo.Sha}");

            commitHeader = commitHeaderBuilder.ToString();
            commitBody = commitInfo.Message.Truncate(300);
        }

        if (lastModified == DateTimeOffset.MinValue)
        {
            lastModified = File.GetLastWriteTimeUtc(sourcePath);
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
