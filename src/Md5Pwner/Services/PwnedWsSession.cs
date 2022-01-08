using System.Text;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace Md5Pwner.Services
{
    public class PwnedWsSession : WsSession
    {
        private readonly ILogger<PwnedWsSession> _logger;

        public PwnedWsSession(ILogger<PwnedWsSession> logger, PwnedWsServer server) : base(server)
        {
            _logger = logger;
        }

        public override void OnWsDisconnected()
        {
            _logger.LogWarning("[{Id}] Connection interrupted", Id);
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

            if (message == "slave")
            {
                _logger.LogInformation("Client {Id} identified!", Id);
                return;
            }
            
            if (message.StartsWith("found"))
            {
                var elements = message.Split(' ');

                _logger.LogInformation("Client {Id} cracked MD5 hash: {MD5} {Solution}", Id, elements[1], elements[2]);
            }
        }

        public void SendStop()
        {
            SendWsContent("stop");
        }

        public void SendExit()
        {
            SendWsContent("exit");
        }

        public void SendSearch(string hash, string begin, string end)
        {
            SendWsContent($"search {hash} {begin} {end}");
        }

        private void SendWsContent(string content)
        {
            _logger.LogInformation("[{Id}]< {Content}", Id, content);
            Send(content);
        }
    }
}
