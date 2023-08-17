using System.Collections.Generic;
using System.IO;

namespace DictCombine;

public static class Exterminator3000
{
    public static void DeleteFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
            DeleteFile(path);
    }

    public static void DeleteFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}