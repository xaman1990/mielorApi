using Microsoft.AspNetCore.Mvc;

namespace AlphaApi.Models
{
    public class NotificationRequest
    {
        public string Token { get; set; }
        // Token del dispositivo al que se enviará la notificación
        public string Title { get; set; }
        public string Body { get; set; }
        public IDictionary<string, string> Data { get; set; }
        //public object Data { get; set; }
        // Datos personalizados opcionales } Integración con el Cliente Registrar el token en el servidor Modifica el método SendTokenToServer para apuntar al endpoint /api/notifications/register-token: csharp Copiar código 
    }

    public class NotificationRequestTopic
    {
        public string Topic { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public Dictionary<string, string> Data { get; set; } // Datos personalizados opcionales
    }
}

