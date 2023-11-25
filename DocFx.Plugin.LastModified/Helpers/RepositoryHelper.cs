namespace DocFx.Plugin.LastModified.Helpers;

using System.IO;
using System.Linq;
using LibGit2Sharp;

/// <summary>
/// Provides methods for repository-related operations.
/// </summary>
public static class RepositoryHelper
{
    /// <summary>
    /// Returns the commit information for the specified file.
    /// </summary>
    /// <param name="repo">The repository to query against.</param>
    /// <param name="srcPath">The path of the file.</param>
    /// <returns>
    /// A <see cref="Commit"/> object containing the information of the commit.
    /// </returns>
    public static Commit? GetCommitInfo(this Repository repo, string srcPath)
    {
        var gitDir = repo.Info.Path
            .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var repoRoot = Path.Join(gitDir, "..");

        if (string.IsNullOrEmpty(repoRoot))
        {
            throw new DirectoryNotFoundException("Cannot obtain the root directory of the repository.");
        }

        var relativePath = Path
            .GetRelativePath(repoRoot, Path.GetFullPath(srcPath))
            .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // See libgit2sharp#1520 for sort issue
        var logEntry = repo.Commits
            .QueryBy(relativePath, new CommitFilter { SortBy = CommitSortStrategies.Topological })
            .FirstOrDefault();

        return logEntry?.Commit;
    }
}
