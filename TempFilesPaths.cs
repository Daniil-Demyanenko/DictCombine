using System;
using System.IO;
using System.Linq;

namespace DictCombine;

public static class TempFilesPaths
{
    public static string TempDirPath => _tempDirPath;

    private static string _tempDirPath = Path.GetTempPath() + String.Join("", Guid.NewGuid().ToString().Take(8)) + "/";

    public static string Next()
    {
        if (!Directory.Exists(_tempDirPath)) Directory.CreateDirectory(_tempDirPath);

        return _tempDirPath + Guid.NewGuid();
    }
}