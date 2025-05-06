using System.ComponentModel.DataAnnotations;

namespace BigData.Apis.Entities
{
    public class InvertedIndexEntry
    {
        [Key]
        public string Word { get; set; } 
        public string IndexResult { get; set; } 
    }
}