using BigData.Apis.Contract;
using BigData.Application.Services;
using BigData.Domain.Entities;
using BigData.Persistance.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BigData.Apis.Services
{
    public class SearchService(ApplicationDbContext dbContext, IIndexingService indexingService)
        : ISearchService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly IIndexingService _indexingService = indexingService;

        public async Task<List<SearchResult>> Search(string query)
        {
            var words = Regex.Split(query.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrEmpty(w))
                .ToList();

            if (!words.Any())
                return new List<SearchResult>();

            var docIds = words.Count == 1
                ? await SearchSingleWord(words[0])
                : SearchPhrase(words);

            var results = new List<SearchResult>();
            foreach (var docId in docIds)
            {
                var doc = _dbContext.Documents.FirstOrDefault(d => d.Id == docId);
                if (doc != null)
                {
                    var snippet = GenerateSnippet(doc.Content, words);
                    var rank = words.Count == 1
                        ? _indexingService.GetIndexForWords(words).Values.FirstOrDefault()?.FirstOrDefault(d => d.DocId == docId).Count ?? 0
                        : 1; // Simplified ranking for phrase search
                    results.Add(new SearchResult
                    {
                        Url = doc.Url,
                        Snippet = snippet,
                        Rank = rank
                    });
                }
            }

            return results.OrderByDescending(r => r.Rank).ToList();
        }

        private async Task<List<int>> SearchSingleWord(string word)
        {
            var entry = await _dbContext.InvertedIndexEntries
                .FirstOrDefaultAsync(e => e.Word == word);
            if (entry == null)
                return new List<int>();

            return entry.IndexResult.Split(';')
                .Select(p => p.Split(':'))
                .Where(p => p.Length == 2)
                .Select(async p => await _dbContext.Documents.FirstOrDefaultAsync(d => d.Url == p[0]))
                .Where(d => d != null)
                .Select(d => d.Id)
                .ToList();
        }

        private List<int> SearchPhrase(List<string> words)
        {
            var index = _indexingService.GetIndexForWords(words);
            if (!index.Any() || words.Any(w => !index.ContainsKey(w)))
                return new List<int>();

            // Find documents containing all words
            var docIdSets = words.Select(w => index[w].Select(d => d.DocId).ToHashSet()).ToList();
            var commonDocIds = docIdSets.Aggregate((a, b) => a.Intersect(b).ToHashSet());

            return commonDocIds.ToList();
        }

        private string GenerateSnippet(string content, List<string> words)
        {
            var snippetLength = 200;
            var contentLower = content.ToLower();
            var firstWord = words[0];
            var index = contentLower.IndexOf(firstWord);

            if (index == -1)
                return content.Substring(0, Math.Min(snippetLength, content.Length));

            var start = Math.Max(0, index - 50);
            var end = Math.Min(content.Length, start + snippetLength);
            var snippet = content.Substring(start, end - start);

            foreach (var word in words)
            {
                snippet = Regex.Replace(snippet, $@"\b{word}\b", $"<mark>{word}</mark>", RegexOptions.IgnoreCase);
            }

            return snippet;
        }


    }

}

