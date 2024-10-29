public interface IEncodingService 
{
    public IAsyncEnumerable<string> GetEncodingInputText(string inputText);
}