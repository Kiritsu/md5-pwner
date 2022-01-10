using System;
using System.Text;
using Microsoft.Extensions.Logging;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Md5Pwner.Services
{
    public class PwnedWsSession : WebSocketBehavior
    {
        private readonly ILogger<PwnedWsSession> _logger;
        private readonly PwnedWsServer _server;
        private readonly PwnedWsService _service;

        public PwnedWsSession(ILogger<PwnedWsSession> logger, PwnedWsServer server, PwnedWsService service)
        {
            _logger = logger;
            _server = server;
            _service = service;
        }

        protected override void OnClose(CloseEventArgs e)
        {
            _logger.LogWarning("[{Id}] Connection interrupted: {Reason}", ID, e.Reason);
            _server.Sessions.Remove(this);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                string message = e.Data;

                if (message == "slave")
                {
                    _logger.LogInformation("Client {Id} identified!", ID);
                    _server.Sessions.Add(this);
                    return;
                }

                if (message.StartsWith("found"))
                {
                    var elements = message.Split(' ');

                    _logger.LogInformation("Client {Id} cracked MD5 hash: {MD5} {Solution}", ID, elements[1], elements[2]);
                    _service.SaveSolution(new() { Hash = elements[1], Value = elements[2], FoundAt = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when receiving a message");
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
            _logger.LogInformation("[{Id}]< {Content}", ID, content);

            try
            {
                Send(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when sending the message");
            }
        }
    }
}
