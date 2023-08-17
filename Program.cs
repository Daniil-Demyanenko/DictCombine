using System;
using System.Linq;

namespace DictCombine;

static class Program
{
    private const int _bufferSize = 1_000_000; //150_000_000;
    private static Dictionary<string, int> _passwords = new Dictionary<string, int>();

    static async Task Main(string[] args)
    {
        string[] inputFilesPaths;
        int bufferSize;

        try
        {
            inputFilesPaths = Directory.GetFiles(args[0]);
            bufferSize = args.Length >= 3 ? int.Parse(args[2]) : _bufferSize;
        }
        catch
        {
            Console.WriteLine(
                $"arguments: [path to directory with dictionaries] [path to output file] [buffer size optional. Default is {_bufferSize}]\n" +
                "Buffer size is the number of records stored in RAM from each file. The more - the faster the program, but more RAM consumption");
            return;
        }

        Console.WriteLine($"Temp folder: {TempFilesPaths.TempDirPath}");

       // using Aggregator aggregator = new(bufferSize, inputFilesPaths, args[1]);
       // var duplicates = await aggregator.Aggregate();
        // foreach (var i in duplicates.OrderBy(x => x.Value))
        //     Console.WriteLine($"{i.Value} {i.Key}");
        Console.WriteLine(await new BigFileSorter(inputFilesPaths[1], bufferSize).Sort());
        Console.ReadLine();
        Directory.Delete(TempFilesPaths.TempDirPath, recursive: true);
    }
}