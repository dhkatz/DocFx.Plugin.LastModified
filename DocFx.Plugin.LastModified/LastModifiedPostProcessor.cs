namespace DocFx.Plugin.LastModified;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using Docfx.Common;
using Docfx.Plugins;
using Processors;

/// <summary>
/// Post-processor responsible for injecting last modified date according to commit or file modified date.
/// </summary>
[Export(nameof(LastModifiedPostProcessor), typeof(IPostProcessor))]
public class LastModifiedPostProcessor : IPostProcessor
{
    private int _addedFiles;

    private IEnumerable<IProcessor> Processors { get; } = new List<IProcessor>
    {
        new ConceptualProcessor(),
        new ManagedReferenceProcessor(),
    };

    /// <inheritdoc />
    public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
        => metadata;

    /// <inheritdoc />
    public Manifest Process(Manifest manifest, string outputFolder)
    {
        var versionInfo = Assembly.GetExecutingAssembly().GetName().Version;

        Logger.LogInfo($"Version: {versionInfo}");
        Logger.LogInfo("Begin adding last modified date to items...");

        foreach (var manifestItem in manifest.Files)
        {
            var processor = Processors.FirstOrDefault(p => p.Supports(manifestItem.Type));
            if (processor == null)
            {
                Logger.LogDiagnostic($"No processor found for {manifestItem.Type}, skipping.");
                continue;
            }

            if (processor.TryProcess(manifest, manifestItem, outputFolder))
            {
                _addedFiles++;
            }
        }

        Logger.LogInfo($"Added modification date to {_addedFiles} articles.");
        return manifest;
    }
}
