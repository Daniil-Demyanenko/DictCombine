namespace DictCombine;

public class Aggregator : IDisposable
{
    private IEnumerable<IAsyncEnumerable<string>> _inputFiles;
    private readonly int _bufferSize;
    private Dictionary<string, int> _passwords = new Dictionary<string, int>();
    private readonly string _tempFilePath;
    private readonly string _outputFilePath;

    public Aggregator(int bufferSize, IEnumerable<string> inputFilesPaths, string outputFilePath)
    {
        (_bufferSize, _outputFilePath) = (bufferSize, outputFilePath);
        _inputFiles = inputFilesPaths.Select(f => File.ReadLinesAsync(f));

        string _tempFilePath = Path.GetTempPath() + Guid.NewGuid();
        File.Create(_tempFilePath);
    }

    public void Dispose()
    {
        File.Delete(_tempFilePath);
    }

    private IEnumerable<string> GetPasswords(IEnumerable<string> passwords)
        => passwords.Take(_bufferSize);

    private bool RememberPasswords(IEnumerable<string> passwords)
    {
        return passwords.All(p =>
        {
            if (!_passwords.TryAdd(p, 1)) _passwords[p]++;
            return true;
        });
    }
}