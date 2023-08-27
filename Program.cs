using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DictCombine;

static class Program
{
    private const int _defaultBufferSize = 15_000_000;

    static async Task Main(string[] args)
    {
        string outputFilePath;
        string[] inputFilesPaths;
        int bufferSize;

        try
        {
            inputFilesPaths = Directory.GetFiles(args[0]);
            outputFilePath = args[1];
            bufferSize = args.Length > 2 ? int.Parse(args[2]) : _defaultBufferSize;
            var tempDirPath = args.Length > 3 ? args[3] : null;
            if (tempDirPath is not null) TempFilesPaths.TempDirPath = tempDirPath;
        }
        catch
        {
            Console.WriteLine(
                "arguments: [path to directory with dictionaries] [path to output file] [buffer size optional] [path to temp directory optional]\n\n" +
                "Buffer size is the number of records stored in RAM from EACH file.\n " +
                $"The more - the faster the program, but more RAM consumption. Default is {_defaultBufferSize}.\n " +
                "The path to the directory with temporary files should be specified if the /tmp directory is mounted in RAM. The files necessary " +
                "for calculations will be created in this directory and will be automatically deleted after the program ends.");
            return;
        }

        if (!CheckOutFile(outputFilePath)) return;
        Exterminator3000.DeleteFile(outputFilePath);

        Console.WriteLine($"Temp folder: {TempFilesPaths.TempDirPath}");

        Stopwatch sw = new();
        sw.Start();
        var duplicateInfo = await new Aggregator(bufferSize, inputFilesPaths, outputFilePath).Aggregate();

        sw.Stop();
        Console.WriteLine($"Found {duplicateInfo.duplicatesCount} duplicates in {duplicateInfo.passCount} rows. \nTotal work time: {sw.Elapsed.TotalSeconds} seconds");

        Directory.Delete(TempFilesPaths.TempDirPath, recursive: true);
    }

    private static bool CheckOutFile(string path)
    {
        if (File.Exists(path))
        {
            Console.Write("The output file already exists. The program will overwrite its contents.\nContinue? [y/n]:");
            var k = Console.ReadKey();
            Console.WriteLine();

            return k.Key == ConsoleKey.Y ? true : false;
        }

        return true;
    }
}