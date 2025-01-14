using AlphaApi.Data;
using AlphaApi.Models;
using AlphaApi.Service.Interfaz;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace AlphaApi.Service.Implementación
{
    public class PushNotificationService: IPushNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PushNotificationService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
        }
        public TokenRequest RegisterToken(RegisterTokenRequest request)
        {
            var response = new TokenRequest();

            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    response.success = false;
                    response.msgError = "El token no puede estar vacío.";
                    return response;
                }

                // Verificar si el token ya existe en la base de datos
                if (_context.Set<RegisterTokenRequest>().Any(t => t.Token == request.Token))
                {
                    response.success = false;
                    response.msgError = "El token ya está registrado.";
                    return response;
                }

                // Crear un nuevo registro para el token
                var nuevoToken = new RegisterTokenRequest
                {
                    Token = request.Token,
                    FechaCreacion = DateTime.Now,
                };

                _context.Set<RegisterTokenRequest>().Add(nuevoToken);
                _context.SaveChanges();

                response.success = true;
                response.msgError = "Token registrado correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.success = false;
                response.msgError = $"Error al registrar el token: {ex.Message}";
                return response;
            }
        }
        public List<string> ObtenerGruposPorSuministro(string suministro)
        {
            try
            {
                // Retornar datos simulados para pruebas
                return new List<string> { "grupo1", "grupo2" };

                // Código real comentado para pruebas:
                // return _context.Grupos
                //     .Where(g => g.SuministroGrupos.Any(sg => sg.Suministro.SuministroId == suministro))
                //     .Select(g => g.Nombre)
                //     .Distinct()
                //     .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener los grupos: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
