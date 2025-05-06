using BigData.Apis.Contract;
using Microsoft.AspNetCore.Mvc;

namespace BigData.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(ISearchService searchService, IIndexingService indexingService) : ControllerBase
    {
        private readonly ISearchService _searchService = searchService;
        private readonly IIndexingService _indexingService = indexingService;

        [HttpGet("search")]
        public IActionResult Search(string query)
        {
            var results =  _searchService.Search(query);
            return Ok(results);
        }

        [HttpPost("index")]
        public async Task<IActionResult> BuildIndex()
        {
            
                await _indexingService.BuildInvertedIndexAsync();
                return Ok("Inverted index built successfully");
            
            
        }
    }
}
