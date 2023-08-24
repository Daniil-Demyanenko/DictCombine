using System;
using System.IO;
using System.Linq;

namespace DictCombine;

public static class TempFilesPaths
{
    public static string TempDirPath
    {
        get => _tempDirPath;
        set => _tempDirPath = CreateTempDirName(value);
    }

    private static string _tempDirPath = CreateTempDirName(Path.GetTempPath());

    public static string Next()
    {
        if (!Directory.Exists(_tempDirPath)) Directory.CreateDirectory(_tempDirPath);

        return _tempDirPath + Guid.NewGuid();
    }

    private static string CreateTempDirName(string path)
    {
        if (!Directory.Exists(path)) throw new Exception("Temp directory is not exists");
        return Path.Combine(path, String.Join("", Guid.NewGuid().ToString().Take(8)) + "/");
    }
}