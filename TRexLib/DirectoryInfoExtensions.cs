using System.IO;

namespace TRexLib;

internal static class DirectoryInfoExtensions
{
    private static readonly string pathSeparator = new(Path.DirectorySeparatorChar, 1);

    public static DirectoryInfo EnsureTrailingSlash(this DirectoryInfo directory)
    {
        if (!directory.FullName
                      .EndsWith(pathSeparator))
        {
            return new DirectoryInfo(directory.FullName + Path.DirectorySeparatorChar);
        }

        return directory;
    }
}