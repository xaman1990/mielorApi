using AlphaApi.Models;
using AlphaApi.Service.Interfaz;
using Microsoft.AspNetCore.Mvc;

namespace AlphaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("generar")]
        public IActionResult GenerarToken([FromQuery] string email)
        {
            try
            {
                var token = _tokenService.GenerarToken(email);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        [HttpGet("validar")]
        public IActionResult ValidarToken([FromQuery] string email, [FromQuery] string token)
        {
            try
            {
                // Validar token usando los parámetros recibidos
                var resultado = _tokenService.ValidarToken(email, token);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
        [HttpGet("tiempo-validez")]
        public IActionResult ObtenerTiempoValidez()
        {
            try
            {
                // Llamar al método del servicio para obtener el tiempo de validez
                var tiempoValidez = _tokenService.ObtenerTiempoValidez();
                return Ok(new { tiempoValidez });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}
