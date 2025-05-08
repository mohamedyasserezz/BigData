using BigData.Apis.Entities;
using BigData.Domain.Entities;
using BigData.Persistance.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using BigData.Apis.Contract;
using System.Text;
namespace BigData.Application.Services
{
   public class IndexingService(ApplicationDbContext dbContext) : IIndexingService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly string _scrapedFilesDirectory = "wikipedia_articles";


        public async Task BuildInvertedIndexAsync()
        {
            // Clear existing data
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE InvertedIndexEntries");
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Documents");

            // Dictionary for inverted index: word -> List<(URL, Count)>
            var invertedIndex = new Dictionary<string, List<(string Url, int Count)>>();
            var documents = new List<Document>();

            // Read all files
            var files = Directory.GetFiles(_scrapedFilesDirectory, "*.txt");
            foreach (var file in files)
            {
                var url = Path.GetFileNameWithoutExtension(file);
                var content = await File.ReadAllTextAsync(file);

                // Store document
                var doc = new Document { Url = url, Content = content };
                documents.Add(doc);

                // Process content: split into words, count frequencies
                var words = Regex.Split(content.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrEmpty(w))
                .Select(w => CleanWord(w))
                .Where(w => !string.IsNullOrEmpty(w) && IsValidWord(w))
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

                // Update inverted index
                foreach (var wordEntry in words)
                {
                    var word = wordEntry.Key;
                    var count = wordEntry.Value;

                    if (!invertedIndex.ContainsKey(word))
                    {
                        invertedIndex[word] = new List<(string Url, int Count)>();
                    }
                    invertedIndex[word].Add((url, count));
                }
            }
            // Save documents
            _dbContext.Documents.AddRange(documents);

            // Save inverted index
            var indexEntries = invertedIndex.Select(entry => new InvertedIndexEntry
            {
                Word = entry.Key,
                IndexResult = string.Join(";", entry.Value.Select(v => $"{v.Url}:{v.Count}"))
            }).ToList();

            _dbContext.InvertedIndexEntries.AddRange(indexEntries);

            await _dbContext.SaveChangesAsync();
        }

        public Dictionary<string, List<(int DocId, int Count)>> GetIndexForWords(IEnumerable<string> words)
        {
            var result = new Dictionary<string, List<(int DocId, int Count)>>();
            foreach (var word in words)
            {
                var entry = _dbContext.InvertedIndexEntries
                    .FirstOrDefault(e => e.Word == word);
                if (entry != null)
                {
                    var docCounts = new List<(int DocId, int Count)>();
                    var pairs = entry.IndexResult.Split(';')
                        .Select(p => p.Split(':'))
                        .Where(p => p.Length == 2);
                    foreach (var pair in pairs)
                    {
                        var url = pair[0];
                        if (int.TryParse(pair[1], out var count))
                        {
                            var doc = _dbContext.Documents.FirstOrDefault(d => d.Url == url);
                            if (doc != null)
                            {
                                docCounts.Add((doc.Id, count));
                            }
                        }
                    }
                    result[word] = docCounts;
                }
            }
            return result;
        }
        private string CleanWord(string word)
        {
            var normalized = word.Normalize(NormalizationForm.FormD);
            var cleaned = new StringBuilder();
            foreach (char c in normalized)
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    cleaned.Append(c);
                }
            }
            return cleaned.ToString().ToLower();
        }
        private bool IsValidWord(string word)
        {
            return !string.IsNullOrEmpty(word) && word.Any(c => char.IsLetterOrDigit(c));
        }
    }
}
