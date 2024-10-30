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
        private static ConcurrentDictionary<string, (WebSocket webSocket, CancellationTokenSource cancellationTokenSource)> _activeWebSockets = new();
    
        private IEncodingService _encodingService;

        public EncodingController(IEncodingService encodingService) 
        {
            _encodingService = encodingService;
        }

        /// <summary>
        /// GET: api/Encoding/{connectionId}
        /// Stablish ws connection with a connection Id.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>process a string.</returns>
        [HttpGet("connect")]
        public async Task ConnectWebSocket([FromQuery] string connectionId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                // Configura un token de cancelación para cada conexión activa
                var cancellationTokenSource = new CancellationTokenSource();
                _activeWebSockets[connectionId] = (webSocket, cancellationTokenSource);

                try
                {
                    // Inicia el bucle de recepción y procesamiento de mensajes
                    await ReceiveAndProcessMessages(webSocket, connectionId, cancellationTokenSource.Token);
                }
                finally
                {
                    _activeWebSockets.TryRemove(connectionId, out _);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task ReceiveAndProcessMessages(WebSocket webSocket, string connectionId, CancellationToken cancellationToken)
        {
            var closeReason = string.Empty;
            try{
                
                var buffer = new byte[1024 * 4];
                
                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexión cerrada", cancellationToken);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        await ProcessTextEncoding(webSocket, receivedText, cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                closeReason = "Operation canceled";
            }
            catch (WebSocketException ex)
            {
                closeReason = "WebSocket Error";
            }
            catch (Exception ex)
            {
                closeReason = "Error";
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, closeReason, CancellationToken.None);
            }
        }

        private async Task ProcessTextEncoding(WebSocket webSocket, string inputText, CancellationToken cancellationToken) 
        {
            try 
            {
                await foreach(var _char in _encodingService.GetEncodingInputText(inputText, cancellationToken)) 
                {
                    if (cancellationToken.IsCancellationRequested) 
                        break;

                    await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(_char)),
                        WebSocketMessageType.Text, 
                        true, 
                        cancellationToken);
                }
            } 
            catch(WebSocketException ex) 
            {
                Console.WriteLine("Error: " + ex.Message);
            } 
        }
        /// <summary>
        /// Cancel a ws request by connectionId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("cancel")]
        public IActionResult CancelProcess([FromBody] CloseConnectionRequest request)
        {
            if (_activeWebSockets.TryGetValue(request.ConnectionId, out var entry))
            {
                entry.cancellationTokenSource.Cancel(); // Cancela el proceso actual
                entry.cancellationTokenSource = new CancellationTokenSource(); // Renueva el token para el próximo proceso
                _activeWebSockets[request.ConnectionId] = (entry.webSocket, entry.cancellationTokenSource);

                return Ok(new { message = "Proceso cancelado, conexión mantenida." });
            }

            return NotFound(new { message = "No se encontró la conexión." });
        }
    }
}