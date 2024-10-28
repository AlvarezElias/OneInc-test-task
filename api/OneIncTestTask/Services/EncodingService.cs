namespace OneIncTestTask.Api.Services 
{
    class GroupCharacter {
        public char Character { get; set; }
        public int Count { get; set; }
    }
    public class EncodingService: IEncodingService 
    {
        public async IAsyncEnumerable<string> GetEncodingInputText(string inputText)
        {
            var groupedCharacters = inputText
                .GroupBy(c => c)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Key}{g.Count()}");


            var resultList = new List<char>();
            foreach(var group in groupedCharacters)
            {
                foreach(var character in group) 
                {
                    await Task.Delay(new Random().Next(1000, 5000));
                    yield return character.ToString();                
                }
            }

            //return / && the input text encoded in base64 format
        }
    }
}