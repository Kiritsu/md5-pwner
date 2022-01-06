using Md5Pwner.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Md5Pwner.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PwnedContext _database;

        public IndexModel(ILogger<IndexModel> logger, PwnedContext database)
        {
            _logger = logger;
            _database = database;
        }

        public void OnGet()
        {
            ViewData["HashesCount"] = _database.Hashes.Count();
        }
    }
}