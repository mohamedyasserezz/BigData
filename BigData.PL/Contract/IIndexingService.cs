namespace BigData.Apis.Contract
{
    public interface IIndexingService
    {
        Task BuildInvertedIndexAsync();
        Dictionary<string, List<(int DocId, int Count)>> GetIndexForWords(IEnumerable<string> words);

    }
}
