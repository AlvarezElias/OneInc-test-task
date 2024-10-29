using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace OneIncTestTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncodingController: ControllerBase 
    {
        private IEncodingService _encodingService;
        private static ConcurrentDictionary<string, WebSocket> _activeWebSockets = new ConcurrentDictionary<string, WebSocket>();

        public EncodingController(IEncodingService encodingService) 
        {
            _encodingService = encodingService;
        }

        /// <summary>
        /// GET: api/Encoding/{inputText}
        /// Endpoint which encode an input text and a connection Id.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="connectionId"></param>
        /// <returns>For each character, return this and a number of ocurrences in the inputText.</returns>
        [HttpGet("connect")]
        public async Task ConnectWebSocket([FromQuery] string inputText, [FromQuery] string connectionId) 
        {
            if(HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _activeWebSockets.TryAdd(connectionId, webSocket);
                await GetTextEncoding(webSocket, inputText, connectionId);
            } 
            else 
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task GetTextEncoding(WebSocket webSocket, string inputText, string connectionId) 
        {
            var receiveBuffer = new byte[1024];
            try 
            {
                while(webSocket.State == WebSocketState.Open)
                {
                    await foreach(var _char in _encodingService.GetEncodingInputText(inputText)) 
                    {
                        var buffer = Encoding.UTF8.GetBytes(_char);
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            } 
            catch(WebSocketException ex) 
            {
                Console.WriteLine("Error: " + ex.Message);
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error on server", CancellationToken.None);
            } 
            finally 
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed", CancellationToken.None);
                _activeWebSockets.TryRemove(connectionId, out _); 
            }
        }
        
        [HttpPut("close")]
        public IActionResult CloseWebSocket([FromBody] string connectionId)
        {
            if (_activeWebSockets.TryGetValue(connectionId, out var webSocket))
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    // Cierra la conexión WebSocket
                    webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection close by request", CancellationToken.None);
                    _activeWebSockets.TryRemove(connectionId, out _);
                }
                
                return Ok("Connection closed");
            }
            
            return NotFound("Not found connection.");
        }
    }
}