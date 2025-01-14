using AlphaApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using AlphaApi.Service.Interfaz;

namespace AlphaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PushNotificationController : ControllerBase
    {
        private readonly string _firebaseEndpoint = "https://fcm.googleapis.com/v1/projects/elor-728d8/messages:send";
        private readonly IPushNotificationService _pushNotificationService;
        public PushNotificationController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
        }

        [HttpPost("register-token")]
        public IActionResult RegisterToken([FromBody] RegisterTokenRequest request)
        {
            try
            {
                // Llamar al servicio para registrar el token
                var response = _pushNotificationService.RegisterToken(request);
                if (!response.success)
                {
                    return BadRequest(response.msgError);
                }

                return Ok(response.msgError);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Body) || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("Título, cuerpo y token son obligatorios.");
            }

            try
            {
                // Cargar las credenciales de la cuenta de servicio
                var credential = GoogleCredential.FromFile("Secrets/firebase-service-account.json")
                                                  .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                // Obtener el token de acceso
                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                // Crear el payload de la notificación
                var payload = new
                {
                    message = new
                    {
                        token = request.Token,
                        notification = new
                        {
                            title = request.Title,
                            body = request.Body
                        },
                        data = request.Data // Opcional: datos personalizados
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_firebaseEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Notificación enviada correctamente.");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, responseContent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al enviar la notificación: {ex.Message}");
            }
        }
        [HttpPost("send-to-topic")]
        public async Task<IActionResult> SendNotificationToTopic([FromBody] NotificationRequestTopic request)
        {
            if (string.IsNullOrEmpty(request.Topic) || string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Body))
            {
                return BadRequest("El tema, título y cuerpo de la notificación son obligatorios.");
            }

            // Validar que la URL de la imagen sea válida si está presente
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                if (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    return BadRequest("La URL de la imagen no es válida.");
                }
            }

            try
            {
                // Cargar las credenciales de la cuenta de servicio
                var credential = GoogleCredential.FromFile("Secrets/firebase-service-account.json")
                                                 .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");

                // Obtener el token de acceso
                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                // Crear el payload para el envío a un tema
                var payload = new
                {
                    message = new
                    {
                        topic = request.Topic, // Tema al que se envía el mensaje
                        notification = new
                        {
                            title = request.Title,
                            body = request.Body,
                            image = string.IsNullOrEmpty(request.ImageUrl) ? null : request.ImageUrl
                        },
                        data = request.Data // Opcional: datos personalizados
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_firebaseEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    return Ok("Notificación enviada correctamente al tema.");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, responseContent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al enviar la notificación al tema: {ex.Message}");
            }
        }
        [HttpGet("{suministro}/grupos-activos")]
        public IActionResult ObtenerGruposActivos(string suministro)
        {
            try
            {
                if (string.IsNullOrEmpty(suministro))
                    return BadRequest("Suministro no puede ser vacío.");

                var grupos = _pushNotificationService.ObtenerGruposPorSuministro(suministro);
                return Ok(grupos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }

}

