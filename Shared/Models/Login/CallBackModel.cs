using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeHelper.Shared.Models.Login;

public class CallBackModel
{
    public string code { get; set; } = null!;

    public string State { get; set; } = null!;

    public string LiffClientId { get; set; } = null!;

    public string LiffRedirectUri { get; set; } = null!;

}
