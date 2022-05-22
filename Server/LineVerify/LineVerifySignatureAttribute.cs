using Microsoft.AspNetCore.Mvc;

namespace LifeHelper.Server.LineVerify
{
    public class LineVerifySignatureAttribute : TypeFilterAttribute
    {
        public LineVerifySignatureAttribute() : base(typeof(LineVerifySignatureFilter)) { }
    }
}
