using System;

namespace DictCombine;

static class Program
{
    private const int _bufferSize = 100_000;
    private static Dictionary<string, int> _passwords = new Dictionary<string, int>();

    static void Main(string[] args)
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

        using Aggregator aggregator = new(bufferSize, inputFilesPaths, args[1]);
    }
}