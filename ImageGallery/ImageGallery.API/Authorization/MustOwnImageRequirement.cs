using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;

namespace ImageGallery.API.Authorization
{
    public class MustOwnImageRequirement :IAuthorizationRequirement
    {
        public MustOwnImageRequirement()
        {

        }
    }
}
