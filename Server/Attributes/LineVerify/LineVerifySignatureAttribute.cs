using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.Attributes.LineVerify
{
    public class LineVerifySignatureAttribute : TypeFilterAttribute
    {
        public LineVerifySignatureAttribute() : base(typeof(LineVerifySignatureFilter)) { }
    }
}
