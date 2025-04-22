using ApiPermissionBasedAuthorization.Models;

namespace ApiPermissionBasedAuthorization.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);

        //Get Token mmken bl username w mmken bl email 
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
    }
}
