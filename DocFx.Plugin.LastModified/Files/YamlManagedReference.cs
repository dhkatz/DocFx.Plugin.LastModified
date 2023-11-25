namespace DocFx.Plugin.LastModified.Files;

/// <summary>
/// A managed reference YAML document.
/// </summary>
public class ManagedReferenceDocument
{
    public required ManagedReferenceItem[]? Items { get; set; }

    public class ManagedReferenceItem
    {
        public required string Uid { get; set; }

        public required string CommentId { get; set; }

        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string FullName { get; set; }

        public required string NameWithType { get; set; }

        public string? Type { get; set; }

        public string? Parent { get; set; }

        public string[]? Children { get; set; }

        public string[]? Langs { get; set; }

        public ManagedReferenceItemSource? Source { get; set; }

        public string[]? Assemblies { get; set; }
    }

    public class ManagedReferenceItemSource
    {
        public required string Id { get; set; }

        public required string Path { get; set; }

        public int? StartLine { get; set; }
    }
}
