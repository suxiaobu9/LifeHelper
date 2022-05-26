using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Attributes.Authorize;

public class LineIdTokenAuthorizeAttribute : TypeFilterAttribute
{
    public LineIdTokenAuthorizeAttribute() : base(typeof(LineIdTokenAuthorizeFilter)) { }

}
