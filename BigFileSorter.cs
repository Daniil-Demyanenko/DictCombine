using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;

namespace DictCombine;

public class BigFileSorter : IDisposable
{
    private string _filePath;
    private readonly int _bufferSize;
    private List<string> _partsPaths = new();
    private readonly object _sync = new object();

    public BigFileSorter(string filePath, int bufferSize)
    {
        (_filePath, _bufferSize) = (filePath, bufferSize);
    }

    /// <summary>
    /// Сортирует строки в указаном файле.
    /// </summary>
    /// <returns>Путь к отсортированному файлу.</returns>
    public async Task<string> Sort()
    {
        await SplitFile();
        SortParts();
        return await SortResult();
    }

    private async Task SplitFile()
    {
        using var reader = new StreamReader(_filePath);

        while (!reader.EndOfStream)
        {
            _partsPaths.Add(TempFilesPaths.Next());

            await using var writer = new StreamWriter(_partsPaths.Last());
            for (int i = 0; i < _bufferSize; i++)
            {
                if (reader.EndOfStream) break;

                await writer.WriteLineAsync(await reader.ReadLineAsync());
            }

        }
    }

    private void SortParts()
    {
        List<string> sortedPaths = new();

        sortedPaths.AddRange(_partsPaths.Select(p =>
        {
            var sorted = File.ReadLinesAsync(p).Where(x => x != String.Empty).OrderBy(x => x).ToEnumerable();
            var newPath = TempFilesPaths.Next();
            File.WriteAllLines(newPath, sorted);

            File.Delete(p);
            return newPath;
        }));

        _partsPaths = sortedPaths;
    }

    private async Task<string> SortResult()
        => await SortAndMargeFiles(_partsPaths);

    /// <summary>
    /// Сортирует файлы, объединяя их в один.
    /// </summary>
    /// <param name="paths">Пути к отсортированным файлам.</param>
    /// <returns>Путь к файлу с результатом.</returns>
    public static async Task<string> SortAndMargeFiles(List<string> paths)
    {
        var resultPath = TempFilesPaths.Next();
        var readers = paths.Select(x => new StreamReader(x)).ToArray();

        try
        {
            var lines = readers.Select(async x => new LineItem((await x.ReadLineAsync())!, x)).ToList();

            await using var writer = new StreamWriter(resultPath);

            while (lines.Count > 0)
            {
                var current = lines.OrderBy(x => x.Result.Line).First();
                await writer.WriteLineAsync(current.Result.Line);

                if (current.Result.Reader.EndOfStream) lines.Remove(current);

                current.Result.Line = (await current.Result.Reader.ReadLineAsync())!;
            }
        }
        finally
        {
            foreach (var r in readers)
            {
                r.Dispose();
            }
        }

        return resultPath;
    }

    public void Dispose()
    {
        foreach (var f in _partsPaths)
            if (File.Exists(f))
                File.Delete(f);
    }
}

file class LineItem
{
    public string Line;
    public readonly StreamReader Reader;

    public LineItem(string line, StreamReader reader) =>
        (Line, Reader) = (line, reader);
}