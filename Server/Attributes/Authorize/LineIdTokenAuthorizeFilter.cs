using LifeHelper.Shared.Models.LIFF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LifeHelper.Server.Attributes.Authorize;

public class LineIdTokenAuthorizeFilter : IAuthorizationFilter
{
    private readonly UserProfile? userProfile;

    public LineIdTokenAuthorizeFilter(IUserProfileService userProfileService)
    {
        this.userProfile = userProfileService.GetUserProfile();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (userProfile == null)
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };

    }
}
