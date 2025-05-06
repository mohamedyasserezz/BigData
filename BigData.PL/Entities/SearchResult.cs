namespace BigData.Domain.Entities
{
    public class SearchResult
    {
        public string Url { get; set; }
        public string Snippet { get; set; } // Highlighted snippet
        public int Rank { get; set; } // Based on word frequency
    }
}
