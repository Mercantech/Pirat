using Microsoft.AspNetCore.Mvc;

namespace WebSocket.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        [HttpGet]
        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var handler = new PiratBridgeHandler(webSocket);
                await handler.HandleConnection();
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
