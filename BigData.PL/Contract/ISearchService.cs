using BigData.Domain.Entities;

namespace BigData.Apis.Contract
{
    public interface ISearchService
    {
        Task<List<SearchResult>> Search(string query);
    }
}
