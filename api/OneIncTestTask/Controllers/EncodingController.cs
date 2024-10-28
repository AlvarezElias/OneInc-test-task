using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace OneIncTestTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncodingController: ControllerBase 
    {
        private IEncodingService _encodingService;

        public EncodingController(IEncodingService encodingService) {
            _encodingService = encodingService;
        }
        /// <summary>
        /// GET: api/Encoding/{inputText}
        /// Endpoint which encode an input text and return one character at a time
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns>An Ok response</returns>
        [HttpGet("connect")]
        public async Task ConnectWebSocket([FromQuery] string inputText) 
        {
            if(HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                
                await GetTextEncoding(webSocket, inputText);
            } 
            else 
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task GetTextEncoding(WebSocket webSocket, string inputText) 
        {
            await foreach(var _char in _encodingService.GetEncodingInputText(inputText)) 
            {
                var buffer = Encoding.UTF8.GetBytes(_char);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed", CancellationToken.None);
        
        }
    }
}