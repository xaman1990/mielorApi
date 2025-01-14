using AlphaApi.Models;

namespace AlphaApi.Service.Interfaz
{
    public interface ITokenService
    {

        TokenRequest GenerarToken(string email);
        TokenRequest ValidarToken(string email, string token);
        int ObtenerTiempoValidez();
    }
}
