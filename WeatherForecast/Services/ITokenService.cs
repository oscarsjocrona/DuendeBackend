using IdentityModel.Client;

namespace WeatherForecast.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);
    }
}
