using System;
using System.Text;
using Microsoft.Extensions.Logging;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Md5Pwner.Services
{
    /// <summary>
    /// Represents a session embedding a client that connects to the websocket server.
    /// </summary>
    public class PwnedWsSession : WebSocketBehavior
    {
        private readonly ILogger<PwnedWsSession> _logger;
        private readonly PwnedWsServer _server;
        private readonly PwnedWsService _service;

        /// <summary>
        /// Creates a new session embedding a client of the websocket server.
        /// </summary>
        /// <param name="logger">Logger of the session.</param>
        /// <param name="server">Websocket server.</param>
        /// <param name="service">Websocket service.</param>
        public PwnedWsSession(ILogger<PwnedWsSession> logger, PwnedWsServer server, PwnedWsService service)
        {
            _logger = logger;
            _server = server;
            _service = service;
        }

        /// <inheritdoc/>
        protected override void OnClose(CloseEventArgs e)
        {
            _logger.LogWarning("[{Id}] Connection interrupted: {Reason}", ID, e.Reason);
            _server.Sessions.Remove(this);
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Sends a stop command to the client.
        /// </summary>
        public void SendStop()
        {
            SendWsContent("stop");
        }

        /// <summary>
        /// Sends an exit command to the client.
        /// </summary>
        public void SendExit()
        {
            SendWsContent("exit");
        }

        /// <summary>
        /// Sends a search command to the client.
        /// </summary>
        /// <param name="hash">Hash to crack.</param>
        /// <param name="begin">Begin character number.</param>
        /// <param name="end">End character number.</param>
        public void SendSearch(string hash, string begin, string end)
        {
            SendWsContent($"search {hash} {begin} {end}");
        }

        /// <summary>
        /// Sends a string to the client.
        /// </summary>
        /// <param name="content">Content to send.</param>
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
