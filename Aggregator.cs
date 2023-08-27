using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace DictCombine;

public class Aggregator
{
    private readonly int _bufferSize;
    private readonly IEnumerable<string> _inputFilesPaths;
    private readonly string _outputFilePath;
    private long _totalPassCount = 0;
    private int _totalDuplicatesCount = 0;

    public Aggregator(int bufferSize, IEnumerable<string> inputFilesPaths, string outputFilePath)
        => (_bufferSize, _outputFilePath, _inputFilesPaths) = (bufferSize, outputFilePath, inputFilesPaths);


    /// <summary>
    /// Собирает файлы из указанной дирректоррии в один.
    /// </summary>
    /// <returns>Кортеж (кол-во найденных дубликатов, общее кол-во паролей до оптимизации)</returns>
    public async Task<(long duplicatesCount, long passCount)> Aggregate()
    {
        var sortedFiles = await SortAllDictionary();

        _totalPassCount = sortedFiles.Select(x => x.AllPassCount).Aggregate((x, y) => x + y);
        WriteLine($"In total, there are {_totalPassCount} lines in the files (including possible duplicates).");

        List<string> partsPaths = new();
        foreach (var i in sortedFiles) partsPaths.AddRange(i.PartsPaths);

        string sortingResultPath = await BigFileSorter.SortAndMargeFiles(partsPaths);

        Exterminator3000.DeleteFiles(partsPaths);

        await RemoveDuplicates(sortingResultPath);

        Exterminator3000.DeleteFile(sortingResultPath);

        return (_totalDuplicatesCount, _totalPassCount);
    }

    private async Task<List<(List<string> PartsPaths, int AllPassCount)>> SortAllDictionary()
    {
        WriteLine("=== Started splitting files into parts that fit in RAM and sorting these parts. ===");

        var sorters = _inputFilesPaths.Select(x => new BigFileSorter(x, _bufferSize));
        var sortTasks = sorters.Select(x => x.SplitFile()).ToList();

        // Список путей к файлам и колчество паролей в каждом исходном файле словаря
        var sortedFiles = new List<(List<string> PartsPaths, int AllPassCount)>();

        while (sortTasks.Count > 0)
        {
            var c = await Task.WhenAny(sortTasks.ToArray());
            sortTasks.Remove(c);
            sortedFiles.Add(c.Result);
        }

        WriteLine("=== Splitting files into parts is over. ===");
        return sortedFiles;
    }

    private async Task RemoveDuplicates(string path)
    {
        WriteLine("=== Search for duplicates. ===");
        long processedCount = 0;

        using var reader = new StreamReader(path);
        await using var writer = new StreamWriter(_outputFilePath);
        string? current = await reader.ReadLineAsync();
        string? newLine = null;

        while (!reader.EndOfStream) // Если следующая строка отличается, записать текущую
        {
            newLine = await reader.ReadLineAsync();

            if (current != newLine)
            {
                await writer.WriteLineAsync(current);
                if (!reader.EndOfStream) current = newLine;
            }
            else _totalDuplicatesCount++;

            if (++processedCount % 1_000_000 == 0)
                WriteLine(
                    $"Processed {processedCount} files out of {_totalPassCount}. Duplicates found {_totalDuplicatesCount}.");
        }

        if (current != newLine) await writer.WriteLineAsync(newLine);
        else _totalDuplicatesCount++;

        writer.Close();
        reader.Close();

        WriteLine("=== The search is over ===");
    }
}