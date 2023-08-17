using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DictCombine;

static class Program
{
    private const int _defaultBufferSize = 150_000_000;
    private static Dictionary<string, int> _passwords = new Dictionary<string, int>();

    static async Task Main(string[] args)
    {
        string outputFilePath;
        string[] inputFilesPaths;
        int bufferSize;

        try
        {
            inputFilesPaths = Directory.GetFiles(args[0]);
            bufferSize = args.Length >= 3 ? int.Parse(args[2]) : _defaultBufferSize;
            outputFilePath = args[1];
        }
        catch
        {
            Console.WriteLine(
                "arguments: [path to directory with dictionaries] [path to output file] [buffer size optional]\n" +
                "Buffer size is the number of records stored in RAM from each file.\n" +
                $"The more - the faster the program, but more RAM consumption. Default is {_defaultBufferSize}");
            return;
        }

        if (!CheckOutFile(outputFilePath)) return;
        Exterminator3000.DeleteFile(outputFilePath);

        Console.WriteLine($"Temp folder: {TempFilesPaths.TempDirPath}");

        var res = await new Aggregator(bufferSize, inputFilesPaths, outputFilePath).Aggregate();

        foreach (var i in res.OrderBy(x => x.Value))
            Console.WriteLine($"{i.Value} | {i.Key}");

        Console.ReadLine();
        Directory.Delete(TempFilesPaths.TempDirPath, recursive: true);
    }

    private static bool CheckOutFile(string path)
    {
        if (File.Exists(path))
        {
            while (true)
            {
                Console.Write(
                    "The output file already exists. The program will overwrite its contents.\nContinue? [y/n]:");
                var k = Console.ReadKey();
                Console.WriteLine();

                if (k.Key == ConsoleKey.Y) return true;
                if (k.Key == ConsoleKey.N) return false;
            }
        }

        return true;
    }
}