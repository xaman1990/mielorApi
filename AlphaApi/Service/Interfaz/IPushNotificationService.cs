using AlphaApi.Models;

namespace AlphaApi.Service.Interfaz
{
    public interface IPushNotificationService
    {
        TokenRequest RegisterToken(RegisterTokenRequest request);
        List<string> ObtenerGruposPorSuministro(string suministro);
    }
}
