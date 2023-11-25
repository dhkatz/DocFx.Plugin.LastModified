namespace DocFx.Plugin.LastModified.Processors;

using Docfx.Plugins;

public interface IProcessor
{
    bool Supports(string type);
    
    void Process(Manifest manifest, ManifestItem manifestItem, string outputFolder);
    
    bool TryProcess(Manifest manifest, ManifestItem manifestItem, string outputFolder);
}