using BigData.Domain.Entities;

namespace BigData.Apis.Contract
{
    public interface ISearchService
    {
        List<SearchResult> Search(string query);
    }
}
