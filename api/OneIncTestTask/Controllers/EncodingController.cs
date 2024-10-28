using Microsoft.AspNetCore.Mvc;

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
        // GET: api/Encoding/{inputText}
        [HttpGet("{inputText}")]
        public async Task<IActionResult> GetTextEncoding(string inputText) 
        {
            var result = await _encodingService.GetEncodingInputText(inputText);

            return Ok(result);
        }
    }
}