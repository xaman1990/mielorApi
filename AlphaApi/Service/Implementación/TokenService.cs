using AlphaApi.Data;
using AlphaApi.Models;
using AlphaApi.Service.Interfaz;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace AlphaApi.Service.Implementación
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public TokenService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        public TokenRequest GenerarToken(string email)
        {
            TokenRequest respuesta = new TokenRequest();
            try
            {
                var tiempoValidez = _context.Set<ConfiguracionToken>().FirstOrDefault()?.TiempoValidez ?? 60;

                // Verificar si ya existe un token no expirado para el correo electrónico
                var tokenExistente = _context.Set<Tokens>()
                    .FirstOrDefault(t => t.Email == email && t.FechaExpiracion > DateTime.Now);

                if (tokenExistente != null)
                {
                    respuesta.success = false;
                    respuesta.msgError = "Ya existe un token válido para este correo.";
                    return respuesta;
                }

                string token;
                do
                {
                    // Generar token de 6 dígitos
                    token = new Random().Next(100000, 999999).ToString();
                }
                while (_context.Set<Tokens>().Any(t => t.Token == token && t.FechaExpiracion > DateTime.Now));

                // Crear nuevo token
                var nuevoToken = new Tokens
                {
                    Email = email,
                    Token = token,
                    FechaCreacion = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddSeconds(tiempoValidez)
                };

                _context.Set<Tokens>().Add(nuevoToken);
                _context.SaveChanges();

                // Intentar enviar el token por correo
                var envioCorreoExitoso = EnviarCorreoAsync(email, token).Result;
                if (!envioCorreoExitoso)
                {
                    respuesta.success = false;
                    respuesta.msgError = "El token fue generado, pero ocurrió un error al enviar el correo.";
                    return respuesta;
                }

                respuesta.success = true;
                respuesta.msgError = "Token creado y enviado correctamente. Verifique la bandeja de entrada del correo ingresado.";
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.success = false;
                respuesta.msgError = $"Error: {ex.Message}";
                return respuesta;
            }
        }

        public async Task<bool> EnviarCorreoAsync(string email, string token)
        {
            // URLs del servicio
            string url = _configuration["ServiceUrls:Url"];

            // Crear el cuerpo del mensaje
            var body = new
            {
                asunto = "Token de validación - MI ELOR APP",
                contenido = $"<h1 style='color: #002060; text-align: center;'>TOKEN DE VALIDACIÓN - MI ELOR APP</h1><p></p><div style='width: 100%; text-align: center; margin: 20px 0;'><div style='display: inline-block; padding: 10px; border: 2px solid #ccc; border-radius: 8px; background-color: #ffffff;'><span style='display: inline-block; padding: 5px 10px; color: #002060; font-size: 18px; font-weight: bold; border-radius: 4px;'>{token}</span></div></div>",
                destinatarios = email
            };

            try
            {
                // Serializar el cuerpo a JSON
                var jsonBody = JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Enviar solicitud POST
                var response = await _httpClient.PostAsync(url, content);

                // Validar respuesta
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public TokenRequest ValidarToken(string email, string token)
        {
            TokenRequest respuesta = new TokenRequest();
            try
            {
                var tokenExistente = _context.Set<Tokens>().FirstOrDefault(t => t.Email.ToLower() == email.ToLower() && t.Token == token);
                if (tokenExistente == null)
                {
                    respuesta.success = false;
                    respuesta.msgError = "El token es incorrecto.";
                    return respuesta;
                }

                if (tokenExistente.FechaUso.HasValue)
                {
                    respuesta.success = false;
                    respuesta.msgError = "El token ya ha sido utilizado.";
                    return respuesta;
                }

                if (tokenExistente.FechaExpiracion < DateTime.Now)
                {
                    respuesta.success = false;
                    respuesta.msgError = "El token ha expirado.";
                    return respuesta;
                }

                // Marcar el token como usado
                tokenExistente.FechaUso = DateTime.Now;
                _context.SaveChanges();

                respuesta.success = true;
                respuesta.msgError = "El token es válido.";
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.success = false;
                respuesta.msgError = $"Error: {ex.Message}";
                return respuesta;
            }
        }
        public int ObtenerTiempoValidez()
        {
            try
            {
                // Obtener el tiempo de validez desde la configuración
                return _context.Set<ConfiguracionToken>().FirstOrDefault()?.TiempoValidez ?? 60;
            }
            catch (Exception ex)
            {
                // Manejar errores si ocurre algún problema al consultar la base de datos
                Console.WriteLine($"Error al obtener el tiempo de validez: {ex.Message}");
                return 60; // Valor por defecto en caso de error
            }
        }
    }
}
