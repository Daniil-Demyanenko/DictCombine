namespace DictCombine;

public class BigFileSorter
{
    private readonly string _inputFilePath;
    private readonly int _bufferSize;

    public BigFileSorter(string filePath, int bufferSize)
    {
        (_inputFilePath, _bufferSize) = (filePath, bufferSize);
    }

    /// <summary>
    /// Сортирует строки в указаном файле.
    /// </summary>
    /// <returns>Путь к отсортированным файлам, кол-во паролей в файле.</returns>
    public async Task<(List<string> Path, long Count)> Sort()
    {
        var (partsPaths, count) = await SplitFile();
        //var path = await SortAndMargeFiles(partsPaths);

        foreach (var f in partsPaths)
            if (File.Exists(f))
                File.Delete(f);

        return (partsPaths, count);
    }

    /// <summary>
    /// Разбивает файл на куски, которые могут поместиться в памяти и сортирует их.
    /// </summary>
    /// <returns>Пути к отсортированным файлам, кол-во паролей в файле.</returns>
    private async Task<(List<string> partsPaths, long Count)> SplitFile()
    {
        List<string> partsPaths = new();
        List<Task> writeTasks = new();
        long count = 0;
        var input = File.ReadLines(_inputFilePath);

        foreach (var i in input.Chunk(_bufferSize))
        {
            var partPath = TempFilesPaths.Next();
            partsPaths.Add(partPath);

            // Сортируем во время нарезки, чтоб сэкономить на чтении/записи файлов
            var sorted = i.OrderBy(x => x).Select(x =>
            {
                ++count;
                return x;
            });

            writeTasks.Add(File.WriteAllLinesAsync(partPath, sorted));
        }

        await Task.WhenAll(writeTasks);

        return (partsPaths, count);
    }

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
            var lines = readers.Select(x => new FileSortItem(x)).ToList();

            await using var writer = new StreamWriter(resultPath);

            while (lines.Count > 0)
            {
                var current = lines.OrderBy(x => x.Line).First();
                await writer.WriteLineAsync(current.Line);

                if (!await current.TryNextAsync()) lines.Remove(current);
            }

            writer.Close();
        }
        finally
        {
            foreach (var r in readers)
                r.Dispose();
        }

        return resultPath;
    }
}

file class FileSortItem
{
    public string? Line { get; private set; }
    private readonly StreamReader _reader;

    public FileSortItem(StreamReader reader) =>
        (Line, _reader) = (reader.ReadLine()!, reader);

    public async Task<bool> TryNextAsync()
    {
        if (_reader.EndOfStream) return false;

        Line = await _reader.ReadLineAsync();
        return true;
    }
}