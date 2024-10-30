using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace OneIncTestTask.Api.Services 
{
    class GroupCharacter {
        public char Character { get; set; }
        public int Count { get; set; }
    }
    public class EncodingService: IEncodingService 
    {
        public async IAsyncEnumerable<string> GetEncodingInputText(string inputText, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var groupedCharacters = inputText
                .GroupBy(c => c)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Key}{g.Count()}").ToArray();
            
            for(var i = 0; i < groupedCharacters.Count(); i++) 
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;
                yield return groupedCharacters[i][0].ToString();

                await Task.Delay(new Random().Next(1000, 5000));
                
                if (cancellationToken.IsCancellationRequested)
                    yield break;
        
                yield return groupedCharacters[i][1].ToString();
                
                await Task.Delay(new Random().Next(1000, 5000));
            }

            if (!cancellationToken.IsCancellationRequested)
            yield return "/" + Convert.ToBase64String(Encoding.UTF8.GetBytes(inputText));
            
            await Task.Delay(new Random().Next(1000, 5000));
            if (!cancellationToken.IsCancellationRequested)
            yield return "FINISH_PROCESS";
        }
    }
}