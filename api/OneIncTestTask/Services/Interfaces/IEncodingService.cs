using System.Runtime.CompilerServices;

public interface IEncodingService 
{
    public IAsyncEnumerable<string> GetEncodingInputText(string inputText, [EnumeratorCancellation] CancellationToken cancellationToken = default);
}