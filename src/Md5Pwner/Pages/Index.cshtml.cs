using Md5Pwner.Database;
using Md5Pwner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Md5Pwner.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PwnedContext _database;
        private readonly PwnedWsServer _server;

        public IndexModel(ILogger<IndexModel> logger, PwnedContext database, PwnedWsServer server)
        {
            _logger = logger;
            _database = database;
            _server = server;
        }

        public void OnGet()
        {
            ViewData["HashesCount"] = _database.Hashes.Count();
            ViewData["WsSessionCount"] = _server.ConnectedSessions;
        }
    }
}