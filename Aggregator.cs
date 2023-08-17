using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DictCombine;

public class Aggregator
{
    /// <summary>
    /// Текущая операция.
    /// </summary>
    public string OperationsStatus { get; private set; } // TODO: текущий статус выполения

    private readonly Dictionary<string, int> _duplicates = new();
    private readonly int _bufferSize;
    private readonly IEnumerable<string> _inputFilesPaths;
    private readonly string _outputFilePath;

    public Aggregator(int bufferSize, IEnumerable<string> inputFilesPaths, string outputFilePath)
    => (_bufferSize, _outputFilePath, _inputFilesPaths, OperationsStatus) = (bufferSize, outputFilePath, inputFilesPaths, "Wait for start");
    

    /// <summary>
    /// Собирает файлы из указанной дирректоррии в один
    /// </summary>
    /// <returns>Словарь со строками, которые встречались несколько раз и их количество</returns>
    public async Task<Dictionary<string, int>> Aggregate()
    {
        var sortedFiles = await SortAllDictionary();

        long passCount = sortedFiles.Select(x => x.AllPassCount).Aggregate((x, y) => x + y);

        List<string> partsPaths = new();
        foreach (var i in sortedFiles) partsPaths.AddRange(i.PartsPaths);

        string sortingResultPath = await BigFileSorter.SortAndMargeFiles(partsPaths);

        Exterminator3000.DeleteFiles(partsPaths);

        await RemoveDuplicates(sortingResultPath);
        
        Exterminator3000.DeleteFile(sortingResultPath);
        
        return _duplicates;
    }

    private async Task<List<(List<string> PartsPaths, long AllPassCount)>> SortAllDictionary()
    {
        var sorters = _inputFilesPaths.Select(x => new BigFileSorter(x, _bufferSize));
        var sortTasks = sorters.AsParallel().Select(x => x.SplitFile()).ToList();
        
        // Список путей к файлам и колчество паролей в каждом исходном файле словаря
        var sortedFiles = new List<(List<string> PartsPaths, long AllPassCount)>();

        while (sortTasks.Count > 0)
        {
            await Task.WhenAny(sortTasks.ToArray());

            var completed = sortTasks.Where(x => x.IsCompleted).ToArray();
            foreach (var t in completed) sortTasks.Remove(t);

            sortedFiles.AddRange(completed.Select(x => x.Result));
        }

        return sortedFiles;
    }

    private async Task RemoveDuplicates(string path)
    {
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
            else AddDuplicate(newLine);
        }

        if (current != newLine) await writer.WriteLineAsync(newLine);
        else AddDuplicate(newLine);

        writer.Close();
        reader.Close();
    }

    private void AddDuplicate(string? item)
    {
        if (item is null) return;

        if (_duplicates.ContainsKey(item)) _duplicates[item]++;
        else _duplicates.Add(item, 1);
    }
}