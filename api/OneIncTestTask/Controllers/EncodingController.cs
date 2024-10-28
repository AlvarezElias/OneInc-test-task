using Microsoft.AspNetCore.Mvc;

namespace OneIncTestTask.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncodingController: ControllerBase 
    {
        // GET: api/Encoding/{inputText}
        [HttpGet("{inputText}")]
        public async Task<IActionResult> GetTextEncoding(string inputText) 
        {
            throw new NotImplementedException();
        }
    }
}