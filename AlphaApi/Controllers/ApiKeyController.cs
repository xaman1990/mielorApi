using AlphaApi.Data;
using AlphaApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlphaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiKeyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Generate()
        {
            var apiKey = new ApiKey
            {
                Key = Guid.NewGuid().ToString(),
                FechaCreacion = DateTime.UtcNow
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            return Ok(apiKey.Key);
        }
    }
}
