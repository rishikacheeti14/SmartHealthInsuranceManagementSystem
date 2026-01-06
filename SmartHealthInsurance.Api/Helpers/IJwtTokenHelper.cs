using SmartHealthInsurance.Api.Models;

namespace SmartHealthInsurance.Api.Helpers
{
    public interface IJwtTokenHelper
    {
        string GenerateToken(User user);
    }
}
