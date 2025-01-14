using System.Linq;
using AlphaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace AlphaApi.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var providedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Falta la API Key.");
                return;
            }

            // Obtener API Keys desde la configuración
            var apiKeysSection = _configuration.GetSection("ApiKeys:Usuarios");
            var apiKeys = apiKeysSection.Get<Dictionary<string, string>>();
            if (apiKeys == null || !apiKeys.Values.Any(v => v == providedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key inválida.");
                return;
            }

            // Opcional: Agregar el usuario al contexto
            var usuario = apiKeys.FirstOrDefault(kvp => kvp.Value == providedApiKey).Key;
            context.Items["Usuario"] = usuario;

            await _next(context);
        }
    }
}
